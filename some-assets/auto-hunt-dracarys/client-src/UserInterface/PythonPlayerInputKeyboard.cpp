# Search in CPythonPlayer::NEW_SetSingleDIKKeyState(int eDIKKey, bool isPress)

	if (NEW_CancelFishing())
		return;

# add before

#ifdef __AUTO_HUNT_DISABLE_COLLISION__
	if (GetAutoHuntStatus())
	{
		m_isSmtMov = false;
		return;
	}
#endif

# Search in CPythonPlayer::NEW_SetSingleDIKKeyState(int eDIKKey, bool isPress)

	switch (eDirKey)

# add before

#ifdef __AUTO_HUNT_DISABLE_COLLISION__
	if (GetAutoHuntStatus())
	{
		m_isSmtMov = false;
		return;
	}
#endif

# Search in CPythonPlayer::NEW_SetMultiDirKeyState(bool isLeft, bool isRight, bool isUp, bool isDown)

	if (!__CanMove())
		return;

# add after

#ifdef __AUTO_HUNT_DISABLE_COLLISION__
	if (GetAutoHuntStatus())
	{
		m_isSmtMov = false;
		return;
	}
#endif

