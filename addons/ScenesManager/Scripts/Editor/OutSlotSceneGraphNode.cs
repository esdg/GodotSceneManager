#if TOOLS
using System;
using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using MoF.Addons.ScenesManager.Constants;
using MoF.Addons.ScenesManager.Extensions;
using MoF.Addons.ScenesManager.Helpers;
using MoF.Addons.ScenesManager.Scripts.Resources;

namespace MoF.Addons.ScenesManager.Scripts.Editor
{
	[Tool]
	public partial class OutSlotSceneGraphNode : HBoxContainer
	{
		[Signal] public delegate void DeleteButtonPressedEventHandler();

		[Signal] public delegate void SelectTransitionChangedEventHandler(bool isTansitionSelected);

		private enum TransitionState { Off, On }

		private OptionButton _signalSelect;
		public OptionButton SignalSelect { get => _signalSelect; }

		private OptionButton _transitionSelect;
		public OptionButton TransitionSelect { get => _transitionSelect; }

		private readonly Array<string> _transitionPaths = new();
		public Array<string> TransitionPaths { get => _transitionPaths; }
		private HBoxContainer _mainContainer;
		private Node _sceneRootNode;
		private string _selectedSignalName;
		private string _selectedTransitionPath;
		private Array<string> _transitionNameList;
		private TextureRect _linkImage;

		private static Texture2D _signalIconTexture;
		private static Texture2D _trashCanIconTexture;

		private static readonly Texture2D[] _transitionIconTextures = new Texture2D[2];
		private static readonly Texture2D[] _linkTextures = new Texture2D[2];

		private TransitionState nodeTransitionSate = TransitionState.Off;

		private TransitionModifier _transitionModifier = new();
		public TransitionModifier TransitionModifier
		{
			get => _transitionModifier;
			set => _transitionModifier = value;
		}

		public OutSlotSceneGraphNode(Node sceneRootNode, Array<string> transitionNameList, string selectedSignalName = "", string selectedTransitionPath = "")
		{
			_sceneRootNode = sceneRootNode;
			_selectedSignalName = selectedSignalName;
			_selectedTransitionPath = selectedTransitionPath;
			_transitionNameList = transitionNameList;
		}

		public override void _Ready()
		{
			LoadTextures();
			InitializeSubMainContainer();
			AddLinkImage();
		}

		private static void LoadTextures()
		{
			_signalIconTexture ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.Icons.SignalIconTexture);
			_transitionIconTextures[(int)TransitionState.Off] ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.Icons.TransitionIconsTexture[(int)TransitionState.Off]);
			_transitionIconTextures[(int)TransitionState.On] ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.Icons.TransitionIconsTexture[(int)TransitionState.On]);
			_trashCanIconTexture ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.Icons.TrashcanIconTexture);
			_linkTextures[(int)TransitionState.Off] ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.SceneGraphNode.linkImagesTransition[(int)TransitionState.Off]);
			_linkTextures[(int)TransitionState.On] ??= ResourceLoader.Load<Texture2D>(Plugin.PathToPlugin + AddonConstants.GraphNode.SceneGraphNode.linkImagesTransition[(int)TransitionState.On]);
		}

		private void InitializeSubMainContainer()
		{
			VBoxContainer vSubMainContainer = new();
			vSubMainContainer.Set("size_flags_horizontal", (int)SizeFlags.ExpandFill);
			AddChild(vSubMainContainer);

			HBoxContainer hSignalContainer = new();
			vSubMainContainer.AddChild(hSignalContainer);

			_transitionSelect = CreateTransitionSelectBox(_transitionNameList, _selectedTransitionPath);
			TransitionSelect.ItemSelected += (index) => { TransitionSelected((int)index); };
			vSubMainContainer.AddChild(TransitionSelect);

			Button deleteBtn = new() { Icon = _trashCanIconTexture };
			deleteBtn.Pressed += () => { EmitSignal(SignalName.DeleteButtonPressed); };
			hSignalContainer.AddChild(deleteBtn);

			_signalSelect = CreateSignalSelectBox(_sceneRootNode, _selectedSignalName);
			hSignalContainer.AddChild(SignalSelect);
		}

		private void TransitionSelected(int index)
		{
			var transition = TransitionSelect.GetItemText(index);
			if (transition == Constants.AddonConstants.TransitionNone)
			{
				nodeTransitionSate = TransitionState.Off;
				_linkImage.Texture = _linkTextures[(int)nodeTransitionSate];
				EmitSignal(SignalName.SelectTransitionChanged, false);
			}
			else
			{
				nodeTransitionSate = TransitionState.On;
				_linkImage.Texture = _linkTextures[(int)TransitionState.On];
				EmitSignal(SignalName.SelectTransitionChanged, true);
			}

		}

		private void AddLinkImage()
		{
			_linkImage = new() { Texture = _linkTextures[(int)nodeTransitionSate], StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered };
			AddChild(_linkImage);
		}

		private static OptionButton CreateSignalSelectBox(Node sceneRootNode, string selectedSignalName = "")
		{
			OptionButton optionButton = GodotHelpers.CreateOptionButton();

			// Get only user-defined signals (automatically excludes all built-in signals)
			// The system dynamically detects built-in signals by analyzing the inheritance chain
			// and comparing against known Godot base classes
			var userSignals = sceneRootNode.GetUserDefinedSignals();

			// Handle case when there are no user-defined signals
			if (userSignals.Count == 0)
			{
				// Add a placeholder item to indicate no custom signals are available
				optionButton.AddItem("(No custom signals found)");
				optionButton.SetItemDisabled(0, true); // Make it non-selectable
				return optionButton;
			}

			// Add all user-defined signals
			foreach (Dictionary signal in userSignals)
			{
				string signalName = (string)signal.Values.First();
				optionButton.AddIconItem(_signalIconTexture, signalName);
			}

			// Handle signal selection
			// If no valid signals are available, the placeholder is already selected
			if (userSignals.Count > 0)
			{
				// select signal if empty change it for none, or find the matching signal
				selectedSignalName = selectedSignalName == "" ? "none" : selectedSignalName;
				bool signalFound = false;

				for (int i = 0; i < optionButton.ItemCount; i++)
				{
					if (optionButton.GetItemText(i) == selectedSignalName)
					{
						optionButton.Select(i);
						signalFound = true;
						break;
					}
				}

				// If the selected signal wasn't found, select the first available signal
				if (!signalFound && optionButton.ItemCount > 0)
				{
					optionButton.Select(0);
				}
			}

			return optionButton;
		}

		/// <summary>
		/// Checks if the currently selected signal is valid (not a placeholder).
		/// </summary>
		/// <returns>True if a valid signal is selected, false if placeholder is selected or no signals available.</returns>
		public bool IsValidSignalSelected()
		{
			if (_signalSelect == null || _signalSelect.ItemCount == 0)
				return false;

			int selectedIndex = _signalSelect.Selected;
			if (selectedIndex < 0)
				return false;

			string selectedText = _signalSelect.GetItemText(selectedIndex);

			// Check if it's the placeholder text
			return !selectedText.StartsWith("(") || !selectedText.EndsWith(")");
		}

		/// <summary>
		/// Gets the currently selected signal name, or null if no valid signal is selected.
		/// </summary>
		/// <returns>The signal name or null if placeholder is selected.</returns>
		public string GetSelectedSignalName()
		{
			if (!IsValidSignalSelected())
				return null;

			return _signalSelect.GetItemText(_signalSelect.Selected);
		}

		private OptionButton CreateTransitionSelectBox(Array<string> transitionNameList, string selectedTransitionPath = "")
		{
			OptionButton optionButton = GodotHelpers.CreateOptionButton();
			optionButton.AddIconItem(_transitionIconTextures[(int)TransitionState.Off], Constants.AddonConstants.TransitionNone);
			foreach (string transitionPath in transitionNameList)
			{
				_transitionPaths.Add(Path.GetFileName(transitionPath));
				optionButton.AddIconItem(_transitionIconTextures[(int)TransitionState.On], GodotHelpers.ToReadableFileName(transitionPath));
			}

			// select signal if provided
			if (selectedTransitionPath != "")
			{
				for (int i = 1; i < optionButton.ItemCount; i++)
				{

					if (_transitionPaths[i - 1] == selectedTransitionPath)
					{
						optionButton.Select(i);
						nodeTransitionSate = TransitionState.On;
						break;
					}
				}
			}
			return optionButton;
		}
	}
}
#endif
