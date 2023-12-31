﻿using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class MiniMapWizard : ScriptableWizard {

        // Will be a subfolder of Application.dataPath and should start with "/"
        private string gameParentFolder = "/Games/";
        private const string imagesFolder = "Images/MiniMap";

        private const string wizardTitle = "Minimap Wizard";

        // the used file path name for the game
        private string fileSystemGameName = string.Empty;

        // user modified variables
        public string gameName = "";
        public int pixelsPerMeter = 10;
        public CameraClearFlags cameraClearFlags = CameraClearFlags.Skybox;
        public Color backgroundColor = Color.black;

        [MenuItem("Tools/AnyRPG/Wizard/MiniMap Wizard")]
        static void CreateWizard() {
            ScriptableWizard.DisplayWizard<MiniMapWizard>(wizardTitle, "Create");
        }

        void OnEnable() {

            SystemConfigurationManager systemConfigurationManager = WizardUtilities.GetSystemConfigurationManager();
            gameName = WizardUtilities.GetGameName(systemConfigurationManager);
            gameParentFolder = WizardUtilities.GetGameParentFolder(systemConfigurationManager, gameName);
        }

        void OnWizardCreate() {

            EditorUtility.DisplayProgressBar(wizardTitle, "Checking parameters...", 0.1f);

            fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);

            EditorUtility.DisplayProgressBar(wizardTitle, "Creating folders...", 0.2f);

            // Setup folder locations
            string fileSystemGameFolder = WizardUtilities.GetGameFileSystemFolder(gameParentFolder, fileSystemGameName);
            string gameImagesFolder = fileSystemGameFolder + "/" + imagesFolder;

            // create missing folders
            WizardUtilities.CreateFolderIfNotExists(Application.dataPath + gameParentFolder);
            WizardUtilities.CreateFolderIfNotExists(fileSystemGameFolder);
            WizardUtilities.CreateFolderIfNotExists(gameImagesFolder);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar(wizardTitle, "Creating Minimap Generator...", 0.3f);

            GameObject minimapGenerator = new GameObject();
            Camera camera = minimapGenerator.AddComponent<Camera>();
            MiniMapGeneratorController miniMapGeneratorController = minimapGenerator.AddComponent<MiniMapGeneratorController>();

            EditorUtility.DisplayProgressBar(wizardTitle, "Calling Minimap Generator...", 0.4f);

            camera.clearFlags = cameraClearFlags;
            camera.backgroundColor = backgroundColor;
            miniMapGeneratorController.mapCamera = camera;
            miniMapGeneratorController.minimapTextureFolder = gameImagesFolder;
            miniMapGeneratorController.pixelsPerMeter = pixelsPerMeter;
            CreateMiniMapTextures(miniMapGeneratorController);

            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar(wizardTitle, "Cleaning Up Minimap Generator...", 0.4f);

            UnityEngine.Object.DestroyImmediate(minimapGenerator);

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(wizardTitle, wizardTitle +" Complete! The minimap image can be found at " + gameImagesFolder, "OK");

        }

        private void CreateMiniMapTextures(MiniMapGeneratorController miniMapGeneratorController) {
            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", 0.1f);
            miniMapGeneratorController.EnableCamera();
            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", 0.5f);
            miniMapGeneratorController.GetSceneBounds();
            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", 0.6f);
            miniMapGeneratorController.CreateFolder();
            EditorUtility.DisplayProgressBar("Generating Minimap...", "Please wait", 0.7f);
            miniMapGeneratorController.CreateMinimapTextures(EditorSceneManager.GetActiveScene().name + ".png");
            EditorUtility.DisplayProgressBar("Generating Minimap...", "Complete", 1);
            EditorUtility.ClearProgressBar();


        }

        void OnWizardUpdate() {
            helpString = "Creates a minimap image of the currently loaded scene";
            fileSystemGameName = WizardUtilities.GetFileSystemGameName(gameName);
            errorString = Validate();
            isValid = (errorString == null || errorString == "");
        }

        string Validate() {
            if (gameName == null || gameName.Trim() == "") {
                return "Game name must not be empty";
            }

            return null;
        }

        public static void DisplayProgressBar(string title, string info, float progress) {
            EditorUtility.DisplayProgressBar(title, info, progress);
        }

    }

}
