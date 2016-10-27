using Dynamo.Graph.Nodes;
using Dynamo.Graph.Presets;
using Dynamo.Logging;
using Dynamo.Selection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dynamo.Graph.Workspaces
{
    internal class Presets
    {
        /// <summary>
        /// Create a new preset state from a set of NodeModels and adds this new state to this presets collection
        /// </summary>
        /// <param name="name">The name of the preset.</param>
        /// <param name="description">A description of what the state does</param>
        /// <param name="currentSelection">A collection of <see cref="NodeModel"/> that are to be serialized in this state</param>
        /// <param name="presets">A collection of <see cref="PresetModel"/>.</param>
        private static PresetModel CreatePresetFromModelIdsCore(string name, string description, IEnumerable<NodeModel> currentSelection, 
            IEnumerable<PresetModel> presets)
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

            ((List<PresetModel>)presets).Add(newstate);
            return newstate;
        }

        internal static void ApplyPreset(PresetModel state, WorkspaceModel workspace, ILogger logger)
        {
            if (state == null)
            {
                logger.Log("Attempted to apply a PresetState that was null");
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

        internal static PresetModel CreatePresetFromModelIds(string name, string description, IEnumerable<Guid> IDSToSave, WorkspaceModel workspace)
        {
            //lookup the nodes by their ID, can also check that we find all of them....
            var nodesFromIDs = workspace.Nodes.Where(node => IDSToSave.Contains(node.GUID));
            //access the presetsCollection and add a new state based on the current selection
            var newpreset = CreatePresetFromModelIdsCore(name, description, nodesFromIDs, workspace.Presets);
            workspace.HasUnsavedChanges = true;
            return newpreset;
        }
    }
}
