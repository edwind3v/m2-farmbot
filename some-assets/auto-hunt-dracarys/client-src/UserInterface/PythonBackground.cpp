# Search in CPythonBackground::CheckAdvancing(CInstanceBase* pInstance)

	if (!IsMapReady())
		return true;

# Add after

#ifdef __AUTO_HUNT_DISABLE_COLLISION__
	if (pInstance->CanSkipCollision())
		return false;
#endif
