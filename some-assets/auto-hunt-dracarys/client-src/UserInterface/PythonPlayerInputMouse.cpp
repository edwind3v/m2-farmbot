# Search in  CPythonPlayer::NEW_SetMouseSmartState(int eMBS, bool isAuto)

	if (!pkInstMain) return;

# add after

#ifdef __AUTO_HUNT_DISABLE_COLLISION__
	if (GetAutoHuntStatus())
	{
		m_isSmtMov = false;
		return;
	}
#endif

