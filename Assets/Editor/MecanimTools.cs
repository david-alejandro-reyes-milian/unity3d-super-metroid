using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Reflection;

public class CopyController : EditorWindow
{
    UnityEditor.Animations.AnimatorController source;
    UnityEditor.Animations.AnimatorController destination;
    UnityEditor.Animations.AnimatorState newState;
    UnityEditor.Animations.AnimatorState destinationState;
    UnityEditor.Animations.AnimatorState defaultState;
    UnityEditor.Animations.AnimatorStateMachine newStateMachine;
    UnityEditor.Animations.AnimatorStateTransition newTransition;
    UnityEditor.Animations.AnimatorTransition newEntryTransition;

    Vector3 entryStatePosition;
    bool isExit;
    string defaultStateName;

    [MenuItem("Helpers/Copy Controller")]
    static void GetWindow()
    {
        EditorWindow.GetWindow(typeof(CopyController));
    }

    void OnGUI()
    {
        source = EditorGUILayout.ObjectField("Source", source, typeof(UnityEditor.Animations.AnimatorController), true, GUILayout.ExpandWidth(true)) as UnityEditor.Animations.AnimatorController;
        destination = EditorGUILayout.ObjectField("Destination", destination, typeof(UnityEditor.Animations.AnimatorController), true, GUILayout.ExpandWidth(true)) as UnityEditor.Animations.AnimatorController;

        GUILayout.Space(100);


        if (GUILayout.Button("Copy", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        {
            if (source != null && destination != null)
                Copy();
        }


        GUILayout.Space(10);

        //if (GUILayout.Button("Clear", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
        //{
        //    if (source != null && destination != null)
        //        Clear();
        //}
    }


    void ClearDestination()
    {
        //Clear Transitions.
        for (int j = 0; j < destination.layers[0].stateMachine.states.Length; j++)
        {
            while (destination.layers[0].stateMachine.states[j].state.transitions.Length != 0)
            {
                for (int k = 0; k < destination.layers[0].stateMachine.states[j].state.transitions.Length; k++)
                {
                    destination.layers[0].stateMachine.states[j].state.RemoveTransition(destination.layers[0].stateMachine.states[j].state.transitions[k]);
                }
            }
        }


        //Clear Parameters.
        while (destination.layers[0].stateMachine.states.Length != 0)
        {
            for (int j = 0; j < destination.layers[0].stateMachine.states.Length; j++)
            {
                destination.layers[0].stateMachine.RemoveState(destination.layers[0].stateMachine.states[j].state);

            }
        }


        //Clear States.
        while (destination.parameters.Length > 0)
        {

            for (int j = 0; j < destination.parameters.Length; j++)
            {
                destination.RemoveParameter(destination.parameters[j]);
            }
        }
    }

    void Copy()
    {

        ClearDestination();


        //Copy Parameters
        for (int i = 0; i < source.parameters.Length; i++)
        {
            destination.AddParameter(source.parameters[i].name, source.parameters[i].type);
        }

        CopyStates(source.layers[0].stateMachine, destination.layers[0].stateMachine, destination.layers[0]);
        CopyTransitions(source.layers[0].stateMachine, destination.layers[0].stateMachine, destination.layers[0]);
        SetDefaultState(source.layers[0].stateMachine, destination.layers[0].stateMachine);
    }


    void SetDefaultState(UnityEditor.Animations.AnimatorStateMachine stateMachine, UnityEditor.Animations.AnimatorStateMachine destinationStateMachine)
    {
        for (int i = 0; i < destinationStateMachine.states.Length; i++)
        {
            if (destinationStateMachine.states[i].state.name == stateMachine.defaultState.name)
                destinationStateMachine.defaultState = destinationStateMachine.states[i].state;
        }

        for (int i = 0; i < destinationStateMachine.stateMachines.Length; i++)
        {
            for (int j = 0; j < destinationStateMachine.stateMachines[i].stateMachine.states.Length; j++)
            {
                if (destinationStateMachine.stateMachines[i].stateMachine.states[j].state.name == stateMachine.defaultState.name)
                    destinationStateMachine.defaultState = destinationStateMachine.stateMachines[i].stateMachine.states[j].state;
            }
        }
    }



    void CopyStates(UnityEditor.Animations.AnimatorStateMachine stateMachine, UnityEditor.Animations.AnimatorStateMachine destinationStateMachine, UnityEditor.Animations.AnimatorControllerLayer destinationLayer)
    {
        destinationStateMachine.exitPosition = stateMachine.exitPosition;
        destinationStateMachine.entryPosition = stateMachine.entryPosition;
        destinationStateMachine.anyStatePosition = stateMachine.anyStatePosition;


        //Copy States
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            newState = destinationStateMachine.AddState(stateMachine.states[i].state.name, stateMachine.states[i].position);
            newState.speed = stateMachine.states[i].state.speed;




            if (stateMachine.states[i].state.motion != null)
            {
                if (stateMachine.states[i].state.motion.GetType() == typeof(AnimationClip))
                {
                    newState.motion = stateMachine.states[i].state.motion;
                }
                else
                {
                    if (stateMachine.states[i].state.motion.GetType() == typeof(UnityEditor.Animations.BlendTree))
                    {
                        UnityEditor.Animations.BlendTree oldBlendTree = stateMachine.states[i].state.motion as UnityEditor.Animations.BlendTree;
                        UnityEditor.Animations.BlendTree newBlendTree = new UnityEditor.Animations.BlendTree();


                        SerializedObject SO = new SerializedObject(newBlendTree);
                        SerializedProperty SP = SO.GetIterator();

                        SP = SO.FindProperty("m_UseAutomaticThresholds");
                        SP.boolValue = false;
                        SO.ApplyModifiedProperties();

                        newBlendTree.blendParameter = oldBlendTree.blendParameter;
                        newBlendTree.blendType = oldBlendTree.blendType;

                        for (int j = 0; j < oldBlendTree.children.Length; j++)
                        {
                            newBlendTree.AddChild(oldBlendTree.children[j].motion, oldBlendTree.children[j].threshold);
                        }

                        for (int j = 0; j < oldBlendTree.children.Length; j++)
                        {
                            UnityEditor.Animations.ChildMotion[] newChildren = newBlendTree.children;
                            newChildren[j].timeScale = oldBlendTree.children[j].timeScale;
                            newChildren[j].position = oldBlendTree.children[j].position;
                            newChildren[j].threshold = oldBlendTree.children[j].threshold;
                            newChildren[j].directBlendParameter = oldBlendTree.children[j].directBlendParameter;
                            newBlendTree.children = newChildren;
                        }

                        newState.motion = newBlendTree;
                    }
                }
            }
        }



        //Set Default State
        for (int i = 0; i < destinationStateMachine.states.Length; i++)
        {
            if (destinationStateMachine.states[i].state.name == defaultStateName)
            {
                destinationStateMachine.defaultState = destinationStateMachine.states[i].state;
                break;
            }
        }


        //Copy Substate Machines
        for (int i = 0; i < stateMachine.stateMachines.Length; i++)
        {
            newStateMachine = new UnityEditor.Animations.AnimatorStateMachine();
            newStateMachine.name = stateMachine.stateMachines[i].stateMachine.name;
            destinationStateMachine.AddStateMachine(newStateMachine.name, stateMachine.stateMachines[i].position);
            CopyStates(stateMachine.stateMachines[i].stateMachine, destinationStateMachine.stateMachines[i].stateMachine, destinationLayer);
        }


        for (int i = 0; i < stateMachine.stateMachines.Length; i++)
        {
            CopyTransitions(stateMachine.stateMachines[i].stateMachine, destinationStateMachine.stateMachines[i].stateMachine, destinationLayer);
        }

    }


    void CopyTransitions(UnityEditor.Animations.AnimatorStateMachine stateMachine, UnityEditor.Animations.AnimatorStateMachine destinationStateMachine, UnityEditor.Animations.AnimatorControllerLayer destinationLayer)
    {

        destinationStateMachine.parentStateMachinePosition = stateMachine.parentStateMachinePosition;


        //Copy Any Transition
        for (int i = 0; i < stateMachine.anyStateTransitions.Length; i++)
        {
            if (stateMachine.anyStateTransitions[i].destinationState != null)
            {
                for (int n = 0; n < destinationStateMachine.states.Length; n++)
                {
                    if (destinationStateMachine.states[n].state.name == stateMachine.anyStateTransitions[i].destinationState.name)
                    {
                        destinationState = destinationStateMachine.states[n].state;
                    }
                }


                newTransition = destinationStateMachine.AddAnyStateTransition(destinationState);
                newTransition.conditions = stateMachine.anyStateTransitions[i].conditions;
                newTransition.canTransitionToSelf = stateMachine.anyStateTransitions[i].canTransitionToSelf;
                newTransition.hasExitTime = stateMachine.anyStateTransitions[i].hasExitTime;
                newTransition.exitTime = stateMachine.anyStateTransitions[i].exitTime;
                newTransition.duration = stateMachine.anyStateTransitions[i].duration;
                newTransition.interruptionSource = stateMachine.anyStateTransitions[i].interruptionSource;
            }
            else
            {
                for (int n = 0; n < destinationStateMachine.stateMachines.Length; n++)
                {
                    if (destinationStateMachine.stateMachines[n].stateMachine.name == stateMachine.anyStateTransitions[i].destinationStateMachine.name)
                    {
                        newStateMachine = destinationStateMachine.stateMachines[n].stateMachine;
                    }
                }

                newTransition = destinationStateMachine.AddAnyStateTransition(newStateMachine);
                newTransition.conditions = stateMachine.anyStateTransitions[i].conditions;
                newTransition.canTransitionToSelf = stateMachine.anyStateTransitions[i].canTransitionToSelf;
                newTransition.hasExitTime = stateMachine.anyStateTransitions[i].hasExitTime;
                newTransition.exitTime = stateMachine.anyStateTransitions[i].exitTime;
                newTransition.duration = stateMachine.anyStateTransitions[i].duration;
                newTransition.interruptionSource = stateMachine.anyStateTransitions[i].interruptionSource;
            }
        }


        //Copy Entry Transitions
        for (int i = 0; i < stateMachine.entryTransitions.Length; i++)
        {
            if (stateMachine.entryTransitions[i].destinationState != null)
            {
                for (int n = 0; n < destinationStateMachine.states.Length; n++)
                {
                    if (destinationStateMachine.states[n].state.name == stateMachine.entryTransitions[i].destinationState.name)
                    {
                        destinationState = destinationStateMachine.states[n].state;
                    }
                }

                newEntryTransition = destinationStateMachine.AddEntryTransition(destinationState);
                newEntryTransition.conditions = stateMachine.entryTransitions[i].conditions;
            }
            else
            {
                for (int n = 0; n < destinationStateMachine.stateMachines.Length; n++)
                {
                    if (destinationStateMachine.stateMachines[n].stateMachine.name == stateMachine.entryTransitions[i].destinationStateMachine.name)
                    {
                        newStateMachine = destinationStateMachine.stateMachines[n].stateMachine;
                    }
                }

                newEntryTransition = destinationStateMachine.AddEntryTransition(newStateMachine);
                newEntryTransition.conditions = stateMachine.anyStateTransitions[i].conditions;
            }
        }



        //Copy State Transitions
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            for (int j = 0; j < stateMachine.states[i].state.transitions.Length; j++)
            {
                if (stateMachine.states[i].state.transitions[j].destinationState != null)
                {
                    for (int n = 0; n < destinationStateMachine.states.Length; n++)
                    {
                        if (destinationStateMachine.states[n].state.name == stateMachine.states[i].state.transitions[j].destinationState.name)
                        {
                            newStateMachine = null;
                            destinationState = destinationStateMachine.states[n].state;
                        }
                    }

                    for (int n = 0; n < destinationLayer.stateMachine.states.Length; n++)
                    {
                        if (destinationLayer.stateMachine.states[n].state.name == stateMachine.states[i].state.transitions[j].destinationState.name)
                        {
                            newStateMachine = null;
                            destinationState = destinationLayer.stateMachine.states[n].state;
                        }
                    }


                    for (int n = 0; n < destinationLayer.stateMachine.stateMachines.Length; n++)
                    {
                        for (int m = 0; m < destinationLayer.stateMachine.stateMachines[n].stateMachine.states.Length; m++)
                        {
                            if (destinationLayer.stateMachine.stateMachines[n].stateMachine.states[m].state.name == stateMachine.states[i].state.transitions[j].destinationState.name)
                            {
                                newStateMachine = null;
                                destinationState = destinationLayer.stateMachine.stateMachines[n].stateMachine.states[m].state;
                            }
                        }
                    }





                    newTransition = destinationStateMachine.states[i].state.AddTransition(destinationState);

                    if (stateMachine.states[i].state.transitions[j].isExit)
                        newTransition.isExit = stateMachine.states[i].state.transitions[j].isExit;

                    newTransition.hasExitTime = stateMachine.states[i].state.transitions[j].hasExitTime;
                    newTransition.exitTime = stateMachine.states[i].state.transitions[j].exitTime;
                    newTransition.canTransitionToSelf = stateMachine.states[i].state.transitions[j].canTransitionToSelf;
                    newTransition.conditions = stateMachine.states[i].state.transitions[j].conditions;
                    newTransition.duration = stateMachine.states[i].state.transitions[j].duration;
                    newTransition.interruptionSource = stateMachine.states[i].state.transitions[j].interruptionSource;
                    newTransition.orderedInterruption = stateMachine.states[i].state.transitions[j].orderedInterruption;


                }
                else
                {
                    for (int n = 0; n < destinationStateMachine.stateMachines.Length; n++)
                    {
                        if (destinationStateMachine.stateMachines[n].stateMachine.name == stateMachine.states[i].state.transitions[j].destinationStateMachine.name)
                        {
                            newStateMachine = destinationStateMachine.stateMachines[n].stateMachine;
                            destinationState = null;
                        }
                    }

                    newTransition = destinationStateMachine.states[i].state.AddTransition(newStateMachine);

                    if (stateMachine.states[i].state.transitions[j].isExit)
                        newTransition.isExit = stateMachine.states[i].state.transitions[j].isExit;

                    newTransition.hasExitTime = stateMachine.states[i].state.transitions[j].hasExitTime;
                    newTransition.canTransitionToSelf = stateMachine.states[i].state.transitions[j].canTransitionToSelf;
                    newTransition.conditions = stateMachine.states[i].state.transitions[j].conditions;
                    newTransition.duration = stateMachine.states[i].state.transitions[j].duration;
                    newTransition.interruptionSource = stateMachine.states[i].state.transitions[j].interruptionSource;
                    newTransition.orderedInterruption = stateMachine.states[i].state.transitions[j].orderedInterruption;
                }
            }
        }
    }
}
