# Add in endfile like other define

#ifdef __AUTO_HUNT__
	PyModule_AddIntConstant(poModule, "__AUTO_HUNT__", 1);
#else
	PyModule_AddIntConstant(poModule, "__AUTO_HUNT__", 0);
#endif
