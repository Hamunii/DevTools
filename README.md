# Dev Tools

This mod interfaces with TestingLib to apply patches and other stuff TestingLib offers. Use `DevTools.cfg` for configuring which TestingLib methods this mod should call.

Note that `TestingLib.xml` needs to be alongside `TestingLib.dll` for this to work, as it contains method documentation, and this mod generates the config file based on what methods it finds in `TestingLib`. This means that `TestingLib` can be updated without updating this mod, and you can still use the new methods with this mod.