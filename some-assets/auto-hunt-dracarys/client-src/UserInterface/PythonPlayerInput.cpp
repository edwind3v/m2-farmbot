#Search

void CPythonPlayer::SetTarget(DWORD dwVID, BOOL bForceChange)

# Change

#ifdef __AUTO_HUNT__
void CPythonPlayer::SetTarget(DWORD dwVID, BOOL bForceChange, bool isAutoAttack)
#else
void CPythonPlayer::SetTarget(DWORD dwVID, BOOL bForceChange)
#endif

#Search

		if (pkInstMain->IsTargetableInstance(*pkInstTarget))

# Change

#ifdef __AUTO_HUNT__
		if (pkInstMain->IsTargetableInstance(*pkInstTarget) || isAutoAttack)
#else
		if (pkInstMain->IsTargetableInstance(*pkInstTarget))
#endif

