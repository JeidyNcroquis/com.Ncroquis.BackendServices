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
                //    e.g. ["Assets","Samples","Backend Services","0.0.3","EntryPoint", ...]
                var parts = assetPath.Split('/');
                if (parts.Length < 5)
                    continue;

                var displayName = parts[2];      // "Backend Services"
                var version = parts[3];          // "0.0.3"
                var sampleName = parts[4];       // "EntryPoint" 등

                // 3) 실제 폴더 "Assets/Samples/.../EntryPoint" 를 딱 한번만 잡아내기
                if (parts.Length == 5 && AssetDatabase.IsValidFolder(assetPath))
                {
                    var srcFolder = assetPath;                         // 원본 폴더 (e.g., Assets/Samples/...)
                    var dstRoot = $"Assets/{OUTPUT_ROOT}";             // 최종 루트 (e.g., Assets/BackendServices)
                    var dstFolder = $"{dstRoot}/{sampleName}";         // 임시 목적지 (e.g., Assets/BackendServices/EntryPoint)

                    // 4) 루트 폴더(Assets/BackendServices) 없으면 만든다
                    if (!AssetDatabase.IsValidFolder(dstRoot))
                        AssetDatabase.CreateFolder("Assets", OUTPUT_ROOT);

                    // 5) 원본이 있고, 목적지 폴더가 없으면 이동
                    if (AssetDatabase.IsValidFolder(srcFolder) && !AssetDatabase.IsValidFolder(dstFolder))
                    {
                        Debug.Log($"[CustomSample] Move\n {srcFolder}\n    → {dstFolder}");
                        var err = AssetDatabase.MoveAsset(srcFolder, dstFolder);

                        if (!string.IsNullOrEmpty(err))
                        {
                            Debug.LogError($"MoveAsset failed: {err}");
                        }
                        else
                        {
                            // --- 폴더 이동 성공 후 추가 작업 ---

                            // 6) "EntryPoint" 샘플의 경우, 모든 내용물을 상위 폴더로 이동하고 자신은 삭제
                            if (sampleName == "EntryPoint")
                            {
                                // 6-a) 이동할 파일 목록 가져오기
                                var entryPointFolder = dstFolder; // "Assets/BackendServices/EntryPoint"
                                var rootFolder = dstRoot;         // "Assets/BackendServices"
                                var filesToMove = Directory.GetFiles(entryPointFolder, "*", SearchOption.AllDirectories);

                                // 6-b) 각 파일을 상위 폴더로 이동
                                foreach (var filePath in filesToMove)
                                {
                                    // .meta 파일은 AssetDatabase가 자동으로 처리하므로 건너뜀
                                    if (filePath.EndsWith(".meta")) continue;

                                    var fileName = Path.GetFileName(filePath);
                                    var sourceFile = $"{entryPointFolder}/{fileName}";
                                    var destFile = $"{rootFolder}/{fileName}";

                                    // 동일한 이름의 파일이 이미 있으면 건너뛰기
                                    if(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), destFile)))
                                    {
                                        Debug.LogWarning($"[CustomSample] File '{fileName}' already exists at root. Skipping move.");
                                        continue;
                                    }
                                    
                                    var moveError = AssetDatabase.MoveAsset(sourceFile, destFile);
                                    if (!string.IsNullOrEmpty(moveError))
                                    {
                                        Debug.LogError($"[CustomSample] Failed to move '{sourceFile}': {moveError}");
                                    }
                                }

                                // 6-c) 내용물이 모두 이동된 EntryPoint 폴더 삭제
                                Debug.Log($"[CustomSample] Deleting empty sample folder: {entryPointFolder}");
                                AssetDatabase.DeleteAsset(entryPointFolder);
                            }

                            // 7) 비어버린 Samples/... 폴더들 정리
                            TryDeleteIfEmpty($"Assets/Samples/{displayName}/{version}");
                            TryDeleteIfEmpty($"Assets/Samples/{displayName}");
                            TryDeleteIfEmpty("Assets/Samples");
                        }
                    }
                }
            }
        }

        // 폴더가 비어 있으면 삭제하는 헬퍼 메서드
        static void TryDeleteIfEmpty(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
                return;

            var diskPath = Path.Combine(Directory.GetCurrentDirectory(), folder);
            // 디렉토리가 존재하고, 내부에 파일이나 폴더가 전혀 없는지 확인
            if (Directory.Exists(diskPath) && Directory.GetFileSystemEntries(diskPath).Length == 0)
            {
                AssetDatabase.DeleteAsset(folder);
                Debug.Log($"[CustomSample] Deleted empty folder: {folder}");
            }
        }
    }
}