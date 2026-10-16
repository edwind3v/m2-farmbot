# Search

	void	SetTarget(DWORD dwVID, BOOL bForceChange = TRUE);

#Change

#ifdef __AUTO_HUNT__
	void	SetTarget(DWORD dwVID, BOOL bForceChange = TRUE, bool isAutoAttack = false);
#else
	void	SetTarget(DWORD dwVID, BOOL bForceChange = TRUE);
#endif



# Search

protected:

# Add before

#ifdef __AUTO_HUNT__
public:
	void	AutoHuntStop();
	void	AutoHuntLoop();
	void	AutoHuntSettings(const CTokenVector& tokenData);
	void	SetAutoHuntDieCheck(const int iSec);
	void	SetAutoHuntSleep(const int iSec);
	bool	GetAutoHuntStatus() { return m_bAutoHuntStatus; }
	const TPixelPosition* GetAutoHuntStartPosition() { return &m_bAutoHuntStartPoint; }
	void	ClearAutoHuntTimer(int iTime);
	void	ClearAutoHunt();
protected:
	DWORD	m_dwAutoHuntSkills[6];
	TPixelPosition	m_bAutoHuntStartPoint;
	bool m_bAutoHuntStatus, m_bAutoHuntMob, m_bAutoHuntStone;
	int	m_iLastAutoHuntDieCheck, m_iAutoHuntSleepTime, m_iAutoHuntEnemyTime, m_iAutoHuntMobTime, m_iAutoHuntStoneTime;
#endif


