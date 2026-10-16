# Search

	NEW_RefreshMouseWalkingDirection();

# add before

#ifdef __AUTO_HUNT__
	AutoHuntLoop();
#endif



# Add in CPythonPlayer::NEW_ClearSkillData(bool bAll) or CPythonPlayer::ClearSkillData() which one u has if has both add both in

#ifdef __AUTO_HUNT__
	ClearAutoHunt();
#endif



# Add

#ifdef __AUTO_HUNT__
#include "PythonMiniMap.h"
void CPythonPlayer::AutoHuntStop()
{
	NEW_Stop();
	__ClearAutoAttackTargetActorID();
	__ClearTarget();
	__ClearReservedAction();
	m_iAutoHuntEnemyTime = 0;
}
void CPythonPlayer::ClearAutoHunt()
{
	memset(&m_dwAutoHuntSkills, 0, sizeof(m_dwAutoHuntSkills));
	m_iAutoHuntMobTime = 0;
	m_iAutoHuntStoneTime = 0;
	m_iAutoHuntEnemyTime = 0;
	m_iLastAutoHuntDieCheck = 0;
	m_iAutoHuntSleepTime = 0;
	m_bAutoHuntMob = false;
	m_bAutoHuntStatus = false;
	m_bAutoHuntStone = false;
}
void CPythonPlayer::AutoHuntSettings(const CTokenVector& tokenData)
{
	if (tokenData.size() != 11)
		return;
	bool bIsActive = false;
	if (!m_bAutoHuntStatus && tokenData[2] == "1")
	{
		bIsActive = true;
		ClearAutoHunt();
	}

	if (tokenData[1] != "-")
	{
		std::vector<std::string> vecSkills{};
		split_argument(tokenData[1].c_str(), vecSkills, "?");
		for (BYTE j = 0; j < vecSkills.size(); ++j)
		{
			if (j <= 5)
				m_dwAutoHuntSkills[j] = atoi(vecSkills[j].c_str());
		}
	}

	m_bAutoHuntStatus = tokenData[2] == "1" ? true : false;
	m_bAutoHuntMob = tokenData[3] == "1" ? true : false;
	m_iAutoHuntMobTime = atoi(tokenData[4].c_str());
	const int iMobTimeMax = atoi(tokenData[5].c_str());
	m_bAutoHuntStone = tokenData[6] == "1" ? true : false;
	m_iAutoHuntStoneTime = atoi(tokenData[7].c_str());
	const int iStoneTimeMax = atoi(tokenData[8].c_str());

	if (tokenData[10] == "1" || !m_bAutoHuntStatus)
		AutoHuntStop();

	CInstanceBase* pkInstMain = NEW_GetMainActorPtr();
#ifdef __AUTO_HUNT_DISABLE_COLLISION__
	if (pkInstMain)
	{
		if(m_bAutoHuntStatus)
			pkInstMain->GetGraphicThingInstance()->EnableSkipCollision();
		else
			pkInstMain->GetGraphicThingInstance()->DisableSkipCollision();
	}
#endif

	if (bIsActive)
	{
		CInstanceBase* pkInstMain = NEW_GetMainActorPtr();
		if (pkInstMain)
			pkInstMain->NEW_GetPixelPosition(&m_bAutoHuntStartPoint);
		CPythonMiniMap::Instance().SetAutoHuntRange(true);
	}
	PyCallClassMemberFunc(m_ppyGameWindow, "BINARY_AutoHuntUpdate", Py_BuildValue("(iiiiiiii)", m_bAutoHuntStatus, m_bAutoHuntMob, m_iAutoHuntMobTime, iMobTimeMax, m_bAutoHuntStone, m_iAutoHuntStoneTime, iStoneTimeMax, tokenData[9] == "1" ? 1 : 0));
}
void CPythonPlayer::SetAutoHuntSleep(const int iSec)
{
	m_iAutoHuntSleepTime = !iSec ? 0 : CPythonApplication::Instance().GetServerTimeStamp() + iSec;
}
void CPythonPlayer::SetAutoHuntDieCheck(const int iSec)
{
	m_iLastAutoHuntDieCheck = !iSec ? 0 : CPythonApplication::Instance().GetServerTimeStamp() + iSec;
}
void CPythonPlayer::ClearAutoHuntTimer(int iTime)
{
	m_iAutoHuntEnemyTime = iTime;
}
bool IsClearPosition(const D3DXVECTOR3& c_v3Dst, const D3DXVECTOR3& c_rv3Src, IPhysicsWorld* pWorld, const int iWhileCount)
{
	D3DXVECTOR3 v3Movement(0.0, 0.0, 0.0);
	const D3DXVECTOR3 c_v3Delta = c_v3Dst - c_rv3Src;
	const D3DXVECTOR3 inc = c_v3Delta / iWhileCount;
	for (int i = 0; i < iWhileCount; ++i)
	{
		if (pWorld->isPhysicalCollision(c_rv3Src + v3Movement))
			return false;
		v3Movement += inc;
	}
	return true;
}
void CPythonPlayer::AutoHuntLoop()
{
	if (!m_bAutoHuntStatus)
		return;

	if (m_iLastAutoHuntDieCheck != 0)
	{
		if (m_iLastAutoHuntDieCheck > CPythonApplication::Instance().GetServerTimeStamp())
			return;
		SetAutoHuntDieCheck(0);
		SetAutoHuntSleep(5); // sleep time
		AutoHuntStop();
		CPythonNetworkStream::Instance().SendChatPacket("/restart_here", CHAT_TYPE_TALKING);
		return;
	}

	if (m_iAutoHuntSleepTime != 0)
	{
		if (m_iAutoHuntSleepTime > CPythonApplication::Instance().GetServerTimeStamp())
			return;
		SetAutoHuntSleep(0);
	}

	CInstanceBase* pkInstMain = NEW_GetMainActorPtr();
	if (pkInstMain)
	{
		CInstanceBase* pkInstVictim = __GetTargetActorPtr();
		for (BYTE j = 0; j < 6; ++j)
		{
			const DWORD skillSlotNumber = m_dwAutoHuntSkills[j];
			if (!skillSlotNumber)
				continue;
			CPythonSkill::TSkillData* pSkillData = NULL;
			if (CPythonSkill::Instance().GetSkillData(GetSkillIndex(skillSlotNumber), &pSkillData))
			{
				const BYTE skillIdx = GetSkillIndex(skillSlotNumber);
				if (pSkillData->IsStandingSkill() && skillIdx != 62 && skillIdx != 76 && skillIdx != 93 && skillIdx != 17)
				{
					if (!IsSkillActive(skillSlotNumber) && !__CheckRestSkillCoolTime(skillSlotNumber) && __CheckSkillUsable(skillSlotNumber))
					{
						__UseSkill(skillSlotNumber);
						return;
					}
					continue;
				}
				if (!pkInstVictim || __CheckRestSkillCoolTime(skillSlotNumber) || !__CheckSkillUsable(skillSlotNumber))
					continue;
				else if (__ProcessEnemySkillTargetRange(*pkInstMain, *pkInstVictim, *pSkillData, skillSlotNumber))
					if (__UseSkill(skillSlotNumber))
						return;
			}
		}

		if (pkInstMain->IsWalking())
		{
			if (pkInstVictim && CPythonCharacterManager::Instance().CanAttackToTarget(pkInstMain, pkInstVictim))
				AutoHuntStop();
			return;
		}

		if (!pkInstVictim || pkInstVictim->IsDead())
		{
		findnewvictim:
			pkInstVictim = CPythonCharacterManager::Instance().FindVictim(pkInstMain, (CPythonMiniMap::Instance().GetAutoHuntRadius() + 40.0) * (4000.0 / 100.0), &m_bAutoHuntStartPoint, m_bAutoHuntMob, m_bAutoHuntStone);
			if (!pkInstVictim)
			{
				TPixelPosition srcPos;
				pkInstMain->NEW_GetPixelPosition(&srcPos);
				const float fDistance = GetDistanceNew(srcPos, m_bAutoHuntStartPoint);
				if (fDistance > 200.0)
					pkInstMain->NEW_MoveToDestPixelPositionDirection(m_bAutoHuntStartPoint);
				return;
			}
			else
			{
				const D3DXVECTOR3& c_rv3Src = pkInstMain->GetGraphicThingInstanceRef().GetPosition();
				const D3DXVECTOR3& c_v3Dst = pkInstVictim->GetGraphicThingInstanceRef().GetPosition();
				if (!IsClearPosition(c_rv3Src, c_v3Dst, IPhysicsWorld::GetPhysicsWorld(), 100))
				{
					CPythonChat::Instance().AppendChat(1, "<Auto-Hunt> Block founded!");
					bool bFoundClear = false;
					for (int i = 0; i < 100; ++i)
					{
						const bool way = random_range(0, 1) == 1 ? true : false;
						D3DXVECTOR3 c_v3DstNew(way ? c_rv3Src.x - random_range(200, 500) : c_rv3Src.x + random_range(200, 500), way ? c_rv3Src.y - random_range(200, 500) : c_rv3Src.y + random_range(200, 500), c_rv3Src.z);
						if (IsClearPosition(c_rv3Src, c_v3DstNew, IPhysicsWorld::GetPhysicsWorld(), 100))
						{
							CPythonChat::Instance().AppendChat(1, "<Auto-Hunt> Position is changed for try again to victim!");
							AutoHuntStop();
							pkInstMain->NEW_MoveToDestPixelPositionDirection(TPixelPosition(+c_v3DstNew.x, -c_v3DstNew.y, +c_v3DstNew.z));
							SetAutoHuntSleep(1);
							break;
						}
					}
					return;
				}
			}

			SetTarget(pkInstVictim->GetVirtualID(), true, true);
			ClearAutoHuntTimer(time(0));
			__OnPressActor(*pkInstMain, m_dwTargetVID, true);
		}
		else
		{
			if (m_iAutoHuntEnemyTime != 0)
			{
				const int iLefTime = time(0) - m_iAutoHuntEnemyTime;
				if (iLefTime == 5)
				{
					AutoHuntStop();
					goto findnewvictim;
				}
			}
		}
	}
}
#endif
