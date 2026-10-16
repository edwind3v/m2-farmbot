# Search

		NEW_AFFECT_QUEST_START_IDX = 1000,

# Add before

#ifdef __AUTO_HUNT__
		NEW_AFFECT_AUTO_HUNT = 830,
#endif

# Search

		AFFECT_CHINA_FIREWORK,

# Add after

#ifdef __AUTO_HUNT__
		AFFECT_AUTO_HUNT,
#endif

# Search

	BOOL					GetFishingRot(int* pirot);

# Add after

#ifdef __AUTO_HUNT__
	bool					CanSkipCollision() { return m_GraphicThingInstance.CanSkipCollision(); }
	CActorInstance*			GetGraphicThingInstance() { return &m_GraphicThingInstance; }
#endif

