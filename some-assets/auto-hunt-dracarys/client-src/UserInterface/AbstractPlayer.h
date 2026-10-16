# Search

	virtual void	SetTarget(DWORD dwVID, BOOL bForceChange = TRUE) = 0;

# Change

#ifdef __AUTO_HUNT__
	virtual void	SetTarget(DWORD dwVID, BOOL bForceChange = TRUE, bool isAutoAttack = false) = 0;
#else
	virtual void	SetTarget(DWORD dwVID, BOOL bForceChange = TRUE) = 0;
#endif
