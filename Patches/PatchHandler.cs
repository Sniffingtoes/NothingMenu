using HarmonyLib;

using System;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace Nothing.Patches
{
    public class PatchHandler
    {
        public static bool IsPatched { get; private set; }
        public static int PatchErrors { get; private set; }

        public static void PatchAll()
        {
            if (!IsPatched)
            {
                instance ??= new Harmony(PluginInfo.GUID);

                foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t != null && t.IsClass && t.GetCustomAttribute<HarmonyPatch>() != null))
                {
                    try
                    {
                        var targetMethod = type.GetMethod("TargetMethod",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                        if (targetMethod != null)
                        {
                            MethodBase resolved = null;
                            try { resolved = targetMethod.Invoke(null, null) as MethodBase; }
                            catch { /* ignore resolution errors */ }

                            if (resolved == null)
                            {
                                Debug.LogWarning($"Skipping patch {type.FullName}: TargetMethod() returned null.");
                                continue;
                            }
                        }

                        instance.CreateClassProcessor(type).Patch();
                    }
                    catch (AmbiguousMatchException ex)
                    {
                        PatchErrors++;
                        Debug.LogError($"Ambiguous method match in {type.FullName} — specify parameter types in the [HarmonyPatch] attribute: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        PatchErrors++;
                        Debug.LogError($"Failed to patch {type.FullName}: {ex}");
                    }
                }

                Debug.Log($"Patched with {PatchErrors} errors");
                IsPatched = true;
            }
        }

        public static void UnpatchAll()
        {
            if (instance != null && IsPatched)
            {
                instance.UnpatchSelf();
                IsPatched = false;
                instance = null;
            }
        }

        public static void ApplyPatch(Type targetClass, string methodName, MethodInfo prefix = null, MethodInfo postfix = null, Type[] parameterTypes = null)
        {
            var original =
                parameterTypes == null ?
                targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) :
                targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, parameterTypes, null);

            if (original == null)
                throw new Exception($"Method '{methodName}' not found on {targetClass.FullName}");

            instance.Patch(original,
                prefix: prefix != null ? new HarmonyMethod(prefix) : null,
                postfix: postfix != null ? new HarmonyMethod(postfix) : null);
        }

        public static void RemovePatch(Type targetClass, string methodName, Type[] parameterTypes = null)
        {
            var original =
                parameterTypes == null ?
                targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) :
                targetClass.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null, parameterTypes, null);

            if (original == null)
                throw new Exception($"Method '{methodName}' not found on {targetClass.FullName}");

            instance.Unpatch(original, HarmonyPatchType.All, instance.Id);
        }

        private static Harmony instance;
        public const string InstanceId = PluginInfo.GUID;
    }
}
