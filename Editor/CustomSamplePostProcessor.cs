using UnityEditor;
using UnityEngine;
using System.IO;

namespace Ncroquis.Backend.Editor
{
    public class CustomSamplePostprocessor : AssetPostprocessor
    {
        // 최종으로 Samples 폴더 대신 생성할 루트 폴더 이름
        const string OUTPUT_ROOT = "BackendServices";

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var assetPath in importedAssets)
            {
                // 1) Assets/Samples/ 로 시작하는 경로만 처리
                if (!assetPath.StartsWith("Assets/Samples/"))
                    continue;

                // 2) 경로를 '/' 로 쪼개서 부분별로 확인
                //    e.g. ["Assets","Samples","Backend Services","0.0.3","Initialize", ...]
                var parts = assetPath.Split('/');
                if (parts.Length < 5)
                    continue;

                var displayName = parts[2];      // "Backend Services"
                var version = parts[3];      // "0.0.3"
                var sampleName = parts[4];      // "Initialize" 등

                // 3) 실제 폴더 "Assets/Samples/.../Initialize" 를 딱 한번만 잡아내기
                if (parts.Length == 5 && AssetDatabase.IsValidFolder(assetPath))
                {
                    var srcFolder = assetPath;                           // 원본
                    var dstRoot = $"Assets/{OUTPUT_ROOT}";             // e.g. Assets/BackendServices
                    var dstFolder = $"{dstRoot}/{sampleName}";           // e.g. Assets/BackendServices/Initialize

                    // 4) 루트 폴더(Assets/BackendServices) 없으면 만든다
                    if (!AssetDatabase.IsValidFolder(dstRoot))
                        AssetDatabase.CreateFolder("Assets", OUTPUT_ROOT);

                    // 5) 원본이 있고, 목적지 폴더가 없으면 이동
                    if (AssetDatabase.IsValidFolder(srcFolder) &&
                        !AssetDatabase.IsValidFolder(dstFolder))
                    {
                        Debug.Log($"[CustomSample] Move\n {srcFolder}\n    → {dstFolder}");
                        var err = AssetDatabase.MoveAsset(srcFolder, dstFolder);
                        if (!string.IsNullOrEmpty(err))
                            Debug.LogError($"MoveAsset failed: {err}");
                        else
                        {
                            // 6) 비어버린 Samples/version, Samples/displayName, Samples 폴더 정리
                            TryDeleteIfEmpty($"Assets/Samples/{displayName}/{version}");
                            TryDeleteIfEmpty($"Assets/Samples/{displayName}");
                            TryDeleteIfEmpty("Assets/Samples");
                        }
                    }
                }
            }
        }

        // 폴더가 비어 있으면 삭제
        static void TryDeleteIfEmpty(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                return;

            // Assets/... → 실제 디스크 경로
            var diskPath = Application.dataPath + folder.Substring("Assets".Length);
            var files = Directory.GetFiles(diskPath, "*", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                AssetDatabase.DeleteAsset(folder);
                Debug.Log($"[CustomSample] Deleted empty folder: {folder}");
            }
        }
    }
}