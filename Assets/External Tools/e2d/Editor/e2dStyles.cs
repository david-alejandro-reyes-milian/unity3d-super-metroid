/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using UnityEditor;
using System.Collections;

/// Class managing all styles used in the editor.
public class e2dStyles
{
	// styles
	public static GUIStyle TextureField; ///< Object field in the editor layout allowing to select a texture.
	public static GUIStyle TextureSelector; ///< An item in the texture selector toolbox.
	public static GUIStyle InfoText; ///< Normal text in info box.
	public static GUIStyle InfoHeadline; ///< Headline of the text in info box.
	public static GUIStyle ErrorText; ///< Normal text in error box.
	public static GUIStyle SceneLabel; ///< Normal text drawn in the Scene view.
	public static GUIStyle SceneError; ///< Error text drawn in the Scene view.
	public static GUIStyle TabButton; ///< Button at the top of a tabbed box.
	public static GUIStyle TabBox; ///< Tabbed box (without the tab).
	public static GUIStyle Popup; ///< Popup field.
	public static GUIStyle Header; ///< Header of a group of elements.
	public static GUIStyle RectArea; ///< Group of rectangle inspector elements.
	public static GUIStyle RectField; ///< One field in the rectangle inspector.
	public static GUIStyle HelpBox; ///< Box for displaying info text.
	public static GUIStyle MiniLabel; ///< Small label.


	/// True if the styles are ready to use.
	private static bool sInited = false;


	/// Are the styles ready to use?
	public static bool Inited { get { return sInited; } }

	/// Creates all GUI styles. It must be called within OnGUI() or OnInspectorGUI() because it's accessing
	/// the current skin.
	public static void Init()
	{
		if (sInited) return;
		sInited = true;

		TextureField = new GUIStyle();
		//TextureField.fixedWidth = 100;
		//TextureField.fixedHeight = 76;

		TextureSelector = new GUIStyle(GUI.skin.button);
		TextureSelector.fixedWidth = 50;
		TextureSelector.fixedHeight = 50;

		InfoText = new GUIStyle("MiniLabel");
		if (sInited)
		{
			InfoText.wordWrap = true;
			InfoText.stretchWidth = true;
		}

		InfoHeadline = new GUIStyle("Label");
		sInited = sInited && InfoHeadline != null;
		if (sInited)
		{
			InfoHeadline.wordWrap = true;
			InfoHeadline.stretchWidth = true;
		}

		ErrorText = new GUIStyle("ErrorLabel");
		sInited = sInited && ErrorText != null;
		if (sInited)
		{
			ErrorText.wordWrap = true;
			ErrorText.stretchWidth = true;
		}

		SceneLabel = new GUIStyle("label");
		sInited = sInited && SceneLabel != null;
		if (sInited)
		{
			SceneLabel.normal.textColor = Color.white;
			SceneLabel.normal.background = (Texture2D)Resources.Load("labelBackground", typeof(Texture2D));
			SceneLabel.fontStyle = FontStyle.Bold;
			SceneLabel.overflow = new RectOffset(2, 3, 0, -2);
		}

		SceneError = new GUIStyle("label");
		sInited = sInited && SceneError != null;
		if (sInited)
		{
			SceneError.normal.textColor = Color.red;
			SceneError.normal.background = (Texture2D)Resources.Load("labelBackground", typeof(Texture2D));
			SceneError.fontStyle = FontStyle.Bold;
			SceneError.fontSize = 20;
			SceneError.overflow = new RectOffset(5, 5, 0, 0);
		}

		TabButton = new GUIStyle("button");
		sInited = sInited && TabButton != null;
		if (sInited)
		{
			TabButton.margin.bottom = 0;
			TabButton.margin.top = 10;
		}

		TabBox = new GUIStyle("HelpBox");
		sInited = sInited && TabBox != null;
		if (sInited)
		{
			TabBox.margin = new RectOffset(TabButton.margin.left, TabButton.margin.left, 0, 0);
			TabBox.padding = new RectOffset(5, 5, 5, 5);
			TabBox.overflow.top = 2;
		}

		Popup = new GUIStyle("Popup");
		sInited = sInited && Popup != null;
		if (sInited)
		{
			Popup.stretchWidth = false;
		}

		Header = new GUIStyle("HeaderLabel");
		sInited = sInited && Header != null;

		RectArea = new GUIStyle();
		RectArea.margin = EditorStyles.numberField.margin;

		RectField = new GUIStyle(EditorStyles.numberField);

		HelpBox = new GUIStyle("HelpBox");
		sInited = sInited && HelpBox != null;

		MiniLabel = new GUIStyle("MiniLabel");
		sInited = sInited && MiniLabel != null;
	}
}