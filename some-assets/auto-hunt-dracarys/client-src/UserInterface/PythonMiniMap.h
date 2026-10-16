# Search

	void HideAtlas();

# Add after

#ifdef __AUTO_HUNT__
	void SetAutoHuntRange(bool bStatus) { m_bAutoHuntRageStatus = bStatus; }
	void ChangeAutoHuntRange(float fVal);
	float GetAutoHuntRadius() { return m_fAutoHuntImageRadius;}
#endif

# Search

	CGraphicExpandedImageInstance	m_MiniMapCameraraphicImageInstance;

# Add after

#ifdef __AUTO_HUNT__
	float							m_fAutoHuntImageRadius;
	bool							m_bAutoHuntRageStatus;
	CGraphicExpandedImageInstance	m_MiniMapAutoHuntRange;
	CGraphicExpandedImageInstance	m_MiniMapAutoHuntStartPoint;
#endif

