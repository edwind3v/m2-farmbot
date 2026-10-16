# Add

#ifdef __AUTO_HUNT__
#include "PythonPlayer.h"
#endif


# Search

	m_GraphicThingInstance.Die();

# add before

#ifdef __AUTO_HUNT__
	if (__IsMainInstance() && CPythonPlayer::Instance().GetAutoHuntStatus())
		CPythonPlayer::Instance().SetAutoHuntDieCheck(12);
#endif

# Search in CInstanceBase::CheckAdvancing()

	if (m_GraphicThingInstance.CanSkipCollision())
	{
		return FALSE;
	}

# Change

#ifndef __AUTO_HUNT_DISABLE_COLLISION__
	if (m_GraphicThingInstance.CanSkipCollision())
	{
		return FALSE;
	}
#endif


# Search in CInstanceBase::CheckAdvancing()

	CPythonBackground& rkBG = CPythonBackground::Instance();

# Add before

#ifdef __AUTO_HUNT_DISABLE_COLLISION__
	if (CanSkipCollision())
		return FALSE;
#endif


