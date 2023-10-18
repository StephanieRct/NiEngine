// Add a struct/class for a reference to a Reaction (or multiple reaction) on a target gameobject, as well as one for multiple target gameobjects
//      with custom UI drawer showing if the reaction/state exist, with a dropdown of existing reaction/state

// Refactor EventProcessor so they dont require an initialize.
//      Remove Owner, Conditions and OnAct. Pass them to each method call instead.

// Add a Log option to ConditionSet.
//  Log false/true/both results.
//  Add toggable icon in the condition set header

// Variables component.
//  List of variable name and IVariable
//  Variable can be referred to with IObjectReference

// Activate Reaction from inspector like it's possible to do with ReactionStateMachine


// Support Fast enter play mode -> https://blog.unity.com/technology/enter-play-mode-faster-in-unity-2019-3


// IO: add CreateObject(io) to ISaveOverride / ISaveOverrideProxy


// Add ReactOnAwake, ReactOnStart components


// ## cleanup
// EventProcessor / EventStateProcessor


// Fix SerializedProperty tracking with VisualElement when Unity fixes this bug:
// VISUALELEMENT.TRACKPROPERTYVALUE DOESN'T INVOKE THE CALLBACK WHEN THE PROPERTY IS UNDER SERIALIZEREFERENCE AND SERIALIZEFIELD ATTRIBUTES
// https://issuetracker.unity3d.com/issues/visualelement-dot-trackpropertyvalue-doesnt-invoke-the-callback-when-the-property-is-under-serializereference-and-serializefield-attributes


// Tool to find string/NiReference objects with variable values in ReactionStateMachines and other ActionSet / StateActionSet.
// Find reference to a gameobject / prefab


// Allow to call (send reaction/ get variable) in children gameobject like so:
// Hierarchy:
//      GoParent
//          Child0 (have a NiVariable called MyVar)
//          Child1 
// Expression
//      Var:[GoParent].[Child0.MyVar]