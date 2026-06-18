# Troubleshooting

## Package does not appear in Package Manager

Confirm the project uses Unity 6 or newer and that `Packages/com.sixstringsyn.rpgtoolkit2d/package.json` exists when installed as an embedded package. For local-path installs, select the package's `package.json`, not the repository root.

## Definitions have duplicate or empty IDs

Open **Tools > RPG Toolkit > Dashboard** and run validation. Use the inspector action to assign a new ID to duplicated content before shipping.

## Save files do not load

Check that every required `ISaveContributor` is registered before loading and that content definitions still have the same `RPGId` values used when the save file was written.

## Dialogue choices are missing

Verify the active `IDialogueContext` contains the flags or values referenced by each choice condition. Missing keys evaluate as unavailable unless the condition explicitly expects an empty value.

## Package removal

Imported samples are copied into the project by Unity Package Manager. Remove imported sample folders manually before removing the package if a clean project tree is required.
