# Add

#ifdef __AUTO_HUNT__
#include "PythonPlayer.h"
bool FindLowerDistance(const std::pair<CInstanceBase*, float>& i, const std::pair<CInstanceBase*, float>& j){return i.second < j.second;}
bool CPythonCharacterManager::CanAttackToTarget(CInstanceBase* pkInstMain, CInstanceBase* pkInstInstance)
{
	if (!pkInstInstance)
		return true;
	else if (pkInstInstance->IsDead())
		return true;
	for (TCharacterInstanceMap::iterator itEx = m_kAliveInstMap.begin(); itEx != m_kAliveInstMap.end(); ++itEx)
	{
		CInstanceBase* nearInstance = itEx->second;
		if (nearInstance->IsPC() && !nearInstance->IsDead() && nearInstance != pkInstMain)// && nearInstance->IsAttacking())
		{
			const float fDistanceCheck = nearInstance->GetDistance(pkInstInstance);
			if (fDistanceCheck < 200.0)
				return true;
		}
	}
	return false;
}
CInstanceBase* CPythonCharacterManager::FindVictim(CInstanceBase* pkInstMain, float fMaxDistance, TPixelPosition* startPoint, const bool m_bAutoHuntMob, const bool m_bAutoHuntStone)
{
	if (!pkInstMain)
		return NULL;
	IPhysicsWorld* pWorld = IPhysicsWorld::GetPhysicsWorld();
	if (!pWorld)
		return NULL;
	const D3DXVECTOR3& c_rv3Src = pkInstMain->GetGraphicThingInstanceRef().GetPosition();
	const DWORD targetPlayerVID = CPythonPlayer::Instance().GetTargetVID();
	D3DXVECTOR3 v3Movement, c_v3Dst;
	TPixelPosition targetPos;
	std::vector<std::pair<CInstanceBase*, float>> m_vecVictimList;
	
	for (TCharacterInstanceMap::iterator itor = m_kAliveInstMap.begin(); itor != m_kAliveInstMap.end(); ++itor)
	{
		CInstanceBase* pkVictim = itor->second;
		if (pkVictim == pkInstMain || pkVictim->IsDead() || pkVictim->GetVirtualID() == targetPlayerVID || !pkInstMain->IsAttackableInstance(*pkVictim))
			continue;
		else if (pkVictim->IsEnemy() && !m_bAutoHuntMob)
			continue;
		else if (pkVictim->IsStone() && !m_bAutoHuntStone)
			continue;
		v3Movement.x = 0.0, v3Movement.y = 0.0, v3Movement.z = 0.0;
		pkVictim->NEW_GetPixelPosition(&targetPos);
		c_v3Dst.x = targetPos.x, c_v3Dst.y = -targetPos.y, c_v3Dst.z = c_rv3Src.z;
		const D3DXVECTOR3 c_v3Delta = c_v3Dst - c_rv3Src;
		const int whileCount = 100;
		const D3DXVECTOR3 inc = c_v3Delta / whileCount;
		bool hasPhysicalBlock = false;
		for (int i = 0; i < whileCount; ++i)
		{
			if (pWorld->isPhysicalCollision(c_rv3Src + v3Movement))
			{
				hasPhysicalBlock = true;
				break;
			}
			v3Movement += inc;
		}
		if (hasPhysicalBlock || CanAttackToTarget(pkInstMain, pkVictim))
			continue;
		const float fDistance = GetDistanceNew(*startPoint, targetPos);
		if (fDistance < fMaxDistance)
			m_vecVictimList.emplace_back(pkVictim, fDistance);
	}
	if (m_vecVictimList.size())
	{
		if (m_vecVictimList.size() > 1)
			std::sort(m_vecVictimList.begin(), m_vecVictimList.end(), FindLowerDistance);
		return m_vecVictimList[0].first;
	}
	return NULL;
}
#endif
