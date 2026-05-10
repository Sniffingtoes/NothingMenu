using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using HarmonyLib;

using Photon.Pun;

using UnityEngine;
using UnityEngine.Networking;

namespace NothingMenu.Menu
{
    internal class Owner : MonoBehaviour
    {
        private const string OwnerListUrl = "https://raw.githubusercontent.com/Sniffingtoes/NothingMenu/refs/heads/main/stuff/owner.txt";
        private const string CrownResourceName = "NothingMenu.Resources.crown.png";
        private const float RefreshInterval = 300f;

        private static readonly Regex IdSplitRegex = new Regex(@"[\s,;]+", RegexOptions.Compiled);

        private readonly HashSet<string> ownerIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, GameObject> badges = new Dictionary<int, GameObject>();
        private Sprite ownerSprite;
        private float nextRefreshTime;
        private bool loadingOwners;

        private void Start()
        {
            ownerSprite = CreateOwnerSprite() ?? CreateFallbackSprite();
            RefreshOwnerList();
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextRefreshTime)
                RefreshOwnerList();

            UpdateBadges();
        }

        private void OnDestroy()
        {
            foreach (GameObject badge in badges.Values)
            {
                if (badge != null)
                    Destroy(badge);
            }

            badges.Clear();
        }

        private void RefreshOwnerList()
        {
            if (loadingOwners) return;

            nextRefreshTime = Time.unscaledTime + RefreshInterval;
            StartCoroutine(LoadOwnerList());
        }

        private IEnumerator LoadOwnerList()
        {
            loadingOwners = true;

            using (UnityWebRequest request = UnityWebRequest.Get(OwnerListUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                    ApplyOwnerList(request.downloadHandler.text);
            }

            loadingOwners = false;
        }

        private void ApplyOwnerList(string text)
        {
            ownerIds.Clear();
            if (string.IsNullOrWhiteSpace(text)) return;

            foreach (string rawLine in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string line = rawLine;
                int commentIndex = line.IndexOf('#');
                if (commentIndex >= 0)
                    line = line.Substring(0, commentIndex);

                foreach (string token in IdSplitRegex.Split(line))
                {
                    string id = token.Trim();
                    if (id.Length > 0)
                        ownerIds.Add(id);
                }
            }
        }

        private void UpdateBadges()
        {
            IReadOnlyList<VRRig> activeRigs = VRRigCache.ActiveRigs;
            if (activeRigs == null || ownerIds.Count == 0)
            {
                ClearBadges();
                return;
            }

            HashSet<int> activeOwnerRigs = new HashSet<int>();
            foreach (VRRig rig in activeRigs)
            {
                if (rig == null || rig == GorillaTagger.Instance.offlineVRRig || rig == VRRig.LocalRig)
                    continue;

                string userId = GetUserId(rig);
                if (string.IsNullOrEmpty(userId) || !ownerIds.Contains(userId))
                    continue;

                int rigId = rig.GetInstanceID();
                activeOwnerRigs.Add(rigId);

                GameObject badge = GetOrCreateBadge(rig, rigId);
                if (badge == null) continue;

                Transform head = rig.headMesh != null ? rig.headMesh.transform : rig.transform;
                badge.transform.position = head.position + Vector3.up * 0.6f;

                Camera camera = Camera.main;
                if (camera != null)
                {
                    badge.transform.LookAt(camera.transform);
                    badge.transform.Rotate(0f, 180f, 0f);
                }
            }

            RemoveInactiveBadges(activeOwnerRigs);
        }

        private string GetUserId(VRRig rig)
        {
            try
            {
                if (rig.Creator != null && !string.IsNullOrEmpty(rig.Creator.UserId))
                    return rig.Creator.UserId;
            }
            catch
            {
            }

            try
            {
                PhotonView photonView = (PhotonView)Traverse.Create(rig).Field("photonView").GetValue();
                return photonView != null && photonView.Owner != null ? photonView.Owner.UserId : null;
            }
            catch
            {
                return null;
            }
        }

        private GameObject GetOrCreateBadge(VRRig rig, int rigId)
        {
            if (badges.TryGetValue(rigId, out GameObject badge) && badge != null)
                return badge;

            badge = new GameObject("NothingOwnerBadge");
            SpriteRenderer renderer = badge.AddComponent<SpriteRenderer>();
            renderer.sprite = ownerSprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 100;
            badge.transform.localScale = Vector3.one * 0.28f;
            badges[rigId] = badge;
            return badge;
        }

        private void RemoveInactiveBadges(HashSet<int> activeOwnerRigs)
        {
            List<int> stale = null;
            foreach (KeyValuePair<int, GameObject> pair in badges)
            {
                if (activeOwnerRigs.Contains(pair.Key)) continue;

                if (pair.Value != null)
                    Destroy(pair.Value);
                (stale ??= new List<int>()).Add(pair.Key);
            }

            if (stale == null) return;
            foreach (int rigId in stale)
                badges.Remove(rigId);
        }

        private void ClearBadges()
        {
            foreach (GameObject badge in badges.Values)
            {
                if (badge != null)
                    Destroy(badge);
            }

            badges.Clear();
        }

        private Sprite CreateOwnerSprite()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(CrownResourceName))
            {
                if (stream == null)
                    return null;

                byte[] imageBytes = new byte[stream.Length];
                stream.Read(imageBytes, 0, imageBytes.Length);

                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(imageBytes))
                    return null;

                texture.filterMode = FilterMode.Bilinear;
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }

        private Sprite CreateFallbackSprite()
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, new Color(1f, 0.78f, 0.12f, 1f));
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
    }
}
