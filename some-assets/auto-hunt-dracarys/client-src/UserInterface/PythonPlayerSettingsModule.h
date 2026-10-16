#note: if you has playersettingmodule in cpp use this method but if in py add like other effect


# Search 

	{ CInstanceBase::EFFECT_AFFECT + 37, "", "d:/ymir work/effect/etc/guild_war_flag/flag_yellow.mse", true},

# Add after


#ifdef __AUTO_HUNT__
	{ CInstanceBase::EFFECT_AFFECT + 39, "Bip01", "locale/common/effect/auto_hunt.mse", true},
#endif
