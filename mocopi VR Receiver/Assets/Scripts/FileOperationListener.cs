using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FileManager : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject;

    [SerializeField]
    private ImageAnimation _imageAnimation;

    private string fileExtension = ".png";
    // Start is called before the first frame update
    void Start()
    {
        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        Debug.Log($"ドキュメントフォルダのパス: {documentsPath}");
        string resourceFolderPath = documentsPath + "/Miburitation";
        if (Directory.Exists(resourceFolderPath))
        {
            SetResources(resourceFolderPath);
        } else
        {
            Debug.LogWarning($"ディレクトリが存在しません: {resourceFolderPath}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetResources(string path)
    {
        string[] files = Directory.GetFiles(path, $"*{fileExtension}");

        List<Sprite> sprites = new List<Sprite>();

        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];

           Texture2D tex2D = convertTexture2D(file);

            // 親オブジェクトのTransformを取得
            Transform parentTransform = targetObject.transform;

            // 親オブジェクトの1番目の子オブジェクトを取得（インデックスは0から始まる）
            Transform childTransform = parentTransform.GetChild(i);

            // 子オブジェクトの名前をログに表示
            Debug.Log("子オブジェクト: " + childTransform.name);

            Image childImage = childTransform.GetComponent<Image>();
            sprites.Add(Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), Vector2.zero));
        }
        Debug.Log("子オブジェクト: " + sprites);
        _imageAnimation.slideSprites = sprites.ToArray();
    }

    private Texture2D convertTexture2D(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex2D = new Texture2D(1280, 720);
        if (tex2D.LoadImage(fileData))
        {
            return tex2D;
        }

        return null;
    }
}
