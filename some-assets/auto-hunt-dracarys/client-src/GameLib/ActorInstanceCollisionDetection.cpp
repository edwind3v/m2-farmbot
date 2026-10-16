#Search in CActorInstance::TestActorCollision(CActorInstance& rVictim)

	D3DXVECTOR3 v3Distance = D3DXVECTOR3(rVictim.m_x - m_x, rVictim.m_y - m_y, rVictim.m_z - m_z);

# add before

#ifdef __AUTO_HUNT_DISABLE_COLLISION__
	if (CanSkipCollision() && !isAttacking())
		return false;
#endif

