using System.Collections;
using UnityEngine;

// Applies a time-limited rainbow overlay effect using a material/shader property
// Intended to work with the shader: Custom/RainbowAddOverlayURP
public class PlayerRainbowEffect : MonoBehaviour
{
    [Header("Target Renderers")]
    [Tooltip("Renderers that receive the rainbow overlay. If empty, searches in children.")]
    public Renderer[] renderers;

    [Header("Shader Property")] 
    [Tooltip("Float property controlling effect intensity (0-1)")]
    public string effectProperty = "_Effect";

    [Header("Timing")] 
    public float duration = 0.75f;
    public AnimationCurve envelope = AnimationCurve.EaseInOut(0, 1, 1, 0); // start strong, fade out

    [Header("Auto Material Setup")]
    [Tooltip("If true, adds a Rainbow overlay material to renderers that have exactly one material.")]
    public bool addOverlayIfMissing = true;
    [Tooltip("Rainbow overlay shader path")] public string overlayShaderPath = "Custom/RainbowAddOverlayURP";

    private int _effectId;
    private MaterialPropertyBlock _mpb;
    private Coroutine _routine;

    private void Awake()
    {
        _effectId = Shader.PropertyToID(effectProperty);
        _mpb = new MaterialPropertyBlock();

        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        }

        if (addOverlayIfMissing)
        {
            TryAddOverlayMaterial();
        }
        SetEffect(0f);
    }

    private void TryAddOverlayMaterial()
    {
        var overlayShader = Shader.Find(overlayShaderPath);
        if (overlayShader == null) return;

        foreach (var r in renderers)
        {
            if (r == null) continue;
            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;

            bool hasProperty = false;
            foreach (var m in mats)
            {
                if (m != null && m.HasProperty(_effectId)) { hasProperty = true; break; }
            }

            if (!hasProperty && mats.Length == 1)
            {
                // Append overlay material so we don't disturb the base lit material
                var newMats = new Material[2];
                newMats[0] = mats[0];
                newMats[1] = new Material(overlayShader);
                r.sharedMaterials = newMats;
            }
        }
    }

    public void Trigger()
    {
        TriggerForSeconds(duration);
    }

    public void TriggerCoin(int value)
    {
        Trigger();
    }

    public void TriggerForSeconds(float seconds)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(CoPulse(seconds));
    }

    private IEnumerator CoPulse(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, seconds));
            float env = Mathf.Clamp01(envelope.Evaluate(u));
            SetEffect(env);
            t += Time.deltaTime;
            yield return null;
        }
        SetEffect(0f);
        _routine = null;
    }

    private void SetEffect(float v)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetFloat(_effectId, v);
            r.SetPropertyBlock(_mpb);
        }
    }
}

