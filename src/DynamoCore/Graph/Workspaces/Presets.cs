using Dynamo.Core;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Presets;
using Dynamo.Selection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dynamo.Graph.Workspaces
{
    internal class Presets
    {
        private readonly List<PresetModel> presets;

        /// <summary>
        ///     A set of input parameter states, this can be used to set the graph to a serialized state.
        /// </summary>
        public IEnumerable<PresetModel> Presets { get { return presets; } }

        /// <summary>
        ///  this method creates a new preset state from a set of NodeModels and adds this new state to this presets collection
        /// </summary>
        /// <param name="name">the name of preset state</param>
        /// <param name="description">a description of what the state does</param>
        /// <param name="currentSelection">a set of NodeModels that are to be serialized in this state</param>
        private static PresetModel AddPresetCore(string name, string description, IEnumerable<NodeModel> currentSelection, List<PresetModel> presets)
        {
            if (currentSelection == null || currentSelection.Count() < 1)
            {
                throw new ArgumentException("currentSelection is empty or null");
            }
            var inputs = currentSelection;

            var newstate = new PresetModel(name, description, inputs);
            if (presets.Any(x => x.GUID == newstate.GUID))
            {
                throw new ArgumentException("duplicate id in collection");
            }

            presets.Add(newstate);
            return newstate;
        }

        /// <summary>
        /// Removes a specified <see cref="PresetModel"/> object from the preset collection of the workspace.
        /// </summary>
        /// <param name="state"><see cref="PresetModel"/> object to remove.</param>
        public static void RemovePreset(PresetModel state, List<PresetModel> presets)
        {
            if (presets.Contains(state))
            {
                presets.Remove(state);
            }
        }

        internal static void ApplyPreset(PresetModel state, WorkspaceModel workspace)
        {
            if (state == null)
            {
                Log("Attempted to apply a PresetState that was null");
                return;
            }
            //start an undoBeginGroup
            using (var undoGroup = workspace.UndoRecorder.BeginActionGroup())
            {
                //reload each node, and record each each modification in the undogroup
                foreach (var node in state.Nodes)
                {
                    //check that node still exists in this workspace,
                    //otherwise bail on this node, check by GUID instead of nodemodel
                    if (workspace.Nodes.Select(x => x.GUID).Contains(node.GUID))
                    {
                        var originalpos = node.Position;
                        var serializedNode = state.SerializedNodes.ToList().Find(x => Guid.Parse(x.GetAttribute("guid")) == node.GUID);
                        //overwrite the xy coords of the serialized node with the current position, so the node is not moved
                        serializedNode.SetAttribute("x", originalpos.X.ToString(CultureInfo.InvariantCulture));
                        serializedNode.SetAttribute("y", originalpos.Y.ToString(CultureInfo.InvariantCulture));
                        serializedNode.SetAttribute("isPinned", node.PreviewPinned.ToString());

                        workspace.UndoRecorder.RecordModificationForUndo(node);
                        workspace.ReloadModel(serializedNode);
                    }
                }
                //select all the modified nodes in the UI
                DynamoSelection.Instance.ClearSelection();
                foreach (var node in state.Nodes)
                {
                    DynamoSelection.Instance.Selection.Add(node);
                }
            }
        }

        internal static PresetModel AddPreset(string name, string description, IEnumerable<Guid> IDSToSave, WorkspaceModel workspace)
        {
            //lookup the nodes by their ID, can also check that we find all of them....
            var nodesFromIDs = workspace.Nodes.Where(node => IDSToSave.Contains(node.GUID));
            //access the presetsCollection and add a new state based on the current selection
            var newpreset = AddPresetCore(name, description, nodesFromIDs);
            workspace.HasUnsavedChanges = true;
            return newpreset;
        }

        /// <summary>
        /// Adds a specified collection <see cref="PresetModel"/> objects to the preset collection of the workspace.
        /// </summary>
        /// <param name="presetCollection"><see cref="PresetModel"/> objects to add.</param>
        public static void ImportPresets(IEnumerable<PresetModel> presetCollection)
        {
            presets.AddRange(presetCollection);
        }
    }
}
