# Search

		if (DamageInfoPacket.damage >= 0)
			pInstTarget->AddDamageEffect(DamageInfoPacket.damage, DamageInfoPacket.flag, bSelf, bTarget);

# Add before

#ifdef __AUTO_HUNT__
		if (!bSelf)
		{
			if (DamageInfoPacket.dwVID == CPythonPlayer::Instance().GetTargetVID())
				CPythonPlayer::Instance().ClearAutoHuntTimer(time(0));
		}
#endif

