# Extension Guide

RPG Toolkit 2D is designed to be customized without editing package source.

## Content definitions

Create new `ScriptableObject` assets from the RPG Toolkit create menus and keep stable `RPGId` values once content ships. Add project-specific wrapper assets when a game needs fields beyond the base package definitions.

## Dialogue and quests

Implement `IDialogueContext` to connect dialogue conditions to game state, or use `WorldStateDialogueContext` for world-state backed flags. Bind `QuestTracker` to a `DialogueRunner` when dialogue commands should advance quest objectives.

## Saving

Implement `ISaveContributor` for project systems. Register contributors with `SaveGameService` so package and project save data are collected through one deterministic flow.

## Interaction

Implement `IInteractable` for custom scene objects. Use `InteractionDetector` for priority selection when several interactables overlap.

## Editor tooling

Prefer adding project-specific editor windows and menu items that call package runtime APIs. Avoid modifying package editor source so future package upgrades remain straightforward.
