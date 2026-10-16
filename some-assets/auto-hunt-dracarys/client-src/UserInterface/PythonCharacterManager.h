# Search

	CInstanceBase* FindClickableInstancePtr();

# Add after

#ifdef __AUTO_HUNT__
	CInstanceBase* FindVictim(CInstanceBase* pkInstMain, float fMaxDistance, TPixelPosition* startPoint, const bool m_bAutoHuntMob, const bool m_bAutoHuntStone);
	bool CanAttackToTarget(CInstanceBase* pkInstMain, CInstanceBase* pkInstInstance);
#endif

