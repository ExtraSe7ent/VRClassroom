using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Photon.Pun;
using System.Linq;

public class DynamicProjector : MonoBehaviourPun
{
    public Renderer screenRenderer;
    
    private List<string> presentationFolders = new List<string>();
    private List<Texture2D> currentSlides = new List<Texture2D>();
    
    private int currentFolderIndex = 0;
    private int currentSlideIndex = 0;
    
    private string rootPath;
    private bool isProjectorOn = false; 

    void Start()
    {
        string basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        if (!basePath.EndsWith("Documents"))
        {
            basePath = Path.Combine(basePath, "Documents");
        }
        
        rootPath = Path.Combine(basePath, "Unity", "File_bai_giang");

        Debug.Log("SYSTEM SEARCHING AT: " + rootPath);

        if (!Directory.Exists(rootPath))
        {
            try 
            {
                Directory.CreateDirectory(rootPath);
                Debug.Log("Created missing directory at: " + rootPath);
            }
            catch
            {
                Debug.LogError("CRITICAL: Could not create or find directory at: " + rootPath);
            }
        }
        else
        {
            Debug.Log("Root directory found successfully!");
            LoadFolders();
        }
    }

    void LoadFolders()
    {
        presentationFolders.Clear();
        string[] directories = Directory.GetDirectories(rootPath);
        presentationFolders.AddRange(directories);

        Debug.Log("Found " + directories.Length + " lecture folders.");

        if (presentationFolders.Count > 0)
        {
            LoadSlidesFromFolder(0); 
        }
    }

    void LoadSlidesFromFolder(int folderIndex)
    {
        if (folderIndex < 0 || folderIndex >= presentationFolders.Count) return;

        currentSlides.Clear();
        string folderPath = presentationFolders[folderIndex];

        if (Directory.Exists(folderPath))
        {
            var filePaths = Directory.GetFiles(folderPath, "*.*")
                                     .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png") || s.EndsWith(".jpeg"));

            foreach (string path in filePaths)
            {
                byte[] fileData = File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData); 
                currentSlides.Add(tex);
            }
            Debug.Log("Loaded " + currentSlides.Count + " slides from lesson: " + Path.GetFileName(folderPath));
        }

        currentFolderIndex = folderIndex;
        currentSlideIndex = 0;
        UpdateScreen();
    }

    void UpdateScreen()
    {
        if (currentSlides.Count > 0 && screenRenderer != null && isProjectorOn)
        {
            screenRenderer.material.mainTexture = currentSlides[currentSlideIndex];
        }
    }


    public void TogglePower() 
    { 
        photonView.RPC("RPC_SetPower", RpcTarget.AllBuffered, !isProjectorOn); 
    }
    
    public void NextFolder() 
    { 
        photonView.RPC("RPC_ChangeFolder", RpcTarget.AllBuffered, 1); 
    }
    
    public void PrevFolder() 
    { 
        photonView.RPC("RPC_ChangeFolder", RpcTarget.AllBuffered, -1); 
    }
    
    public void NextSlide() 
    { 
        photonView.RPC("RPC_ChangeSlide", RpcTarget.AllBuffered, 1); 
    }
    
    public void PrevSlide() 
    { 
        photonView.RPC("RPC_ChangeSlide", RpcTarget.AllBuffered, -1); 
    }


    [PunRPC] 
    void RPC_SetPower(bool state) 
    { 
        isProjectorOn = state; 
        if (screenRenderer != null) 
        { 
            screenRenderer.enabled = isProjectorOn; 
            if (isProjectorOn) UpdateScreen(); 
        } 
    }

    [PunRPC] 
    void RPC_ChangeFolder(int direction) 
    { 
        if (presentationFolders.Count == 0) return; 
        
        int newIndex = currentFolderIndex + direction; 
        
        if (newIndex >= presentationFolders.Count) newIndex = 0; 
        if (newIndex < 0) newIndex = presentationFolders.Count - 1; 
        
        LoadSlidesFromFolder(newIndex); 
    }

    [PunRPC] 
    void RPC_ChangeSlide(int direction) 
    { 
        if (currentSlides.Count == 0) return; 
        
        int newIndex = currentSlideIndex + direction; 
        
        if (newIndex >= currentSlides.Count) newIndex = currentSlides.Count - 1; 
        if (newIndex < 0) newIndex = 0; 
        
        currentSlideIndex = newIndex; 
        UpdateScreen(); 
    }
}