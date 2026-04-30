using UnityEngine;

public class PlayerCharacterSwapper : MonoBehaviour
{
    [SerializeField] private GameObject replacementCharacter;
    [SerializeField] private Vector3 localPosition = Vector3.zero;
    [SerializeField] private Vector3 localEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 localScale = Vector3.one;

    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int MotionSpeed = Animator.StringToHash("MotionSpeed");
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int FreeFall = Animator.StringToHash("FreeFall");

    private GameObject spawnedCharacter;
    private Animator sourceAnimator;
    private Animator replacementAnimator;

    private void Awake()
    {
        sourceAnimator = GetComponent<Animator>();
        SpawnReplacementCharacter();
        HideOriginalRenderers();
    }

    private void LateUpdate()
    {
        SyncStarterAssetsAnimator();
    }

    private void SpawnReplacementCharacter()
    {
        if (replacementCharacter == null)
        {
            Debug.LogWarning("PlayerCharacterSwapper has no replacement character assigned.", this);
            return;
        }

        spawnedCharacter = Instantiate(replacementCharacter, transform);
        spawnedCharacter.name = replacementCharacter.name;
        spawnedCharacter.transform.localPosition = localPosition;
        spawnedCharacter.transform.localRotation = Quaternion.Euler(localEulerAngles);
        spawnedCharacter.transform.localScale = localScale;

        DisableReplacementColliders();
        ConfigureReplacementAnimator();
    }

    private void HideOriginalRenderers()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            if (spawnedCharacter != null && renderer.transform.IsChildOf(spawnedCharacter.transform))
            {
                continue;
            }

            renderer.enabled = false;
        }
    }

    private void DisableReplacementColliders()
    {
        Collider[] colliders = spawnedCharacter.GetComponentsInChildren<Collider>(true);

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void ConfigureReplacementAnimator()
    {
        replacementAnimator = spawnedCharacter.GetComponentInChildren<Animator>();

        if (sourceAnimator == null || replacementAnimator == null || sourceAnimator.runtimeAnimatorController == null)
        {
            return;
        }

        replacementAnimator.runtimeAnimatorController = sourceAnimator.runtimeAnimatorController;
        replacementAnimator.applyRootMotion = false;
        replacementAnimator.updateMode = sourceAnimator.updateMode;
        replacementAnimator.cullingMode = sourceAnimator.cullingMode;
    }

    private void SyncStarterAssetsAnimator()
    {
        if (sourceAnimator == null || replacementAnimator == null)
        {
            return;
        }

        replacementAnimator.SetFloat(Speed, sourceAnimator.GetFloat(Speed));
        replacementAnimator.SetFloat(MotionSpeed, sourceAnimator.GetFloat(MotionSpeed));
        replacementAnimator.SetBool(Grounded, sourceAnimator.GetBool(Grounded));
        replacementAnimator.SetBool(Jump, sourceAnimator.GetBool(Jump));
        replacementAnimator.SetBool(FreeFall, sourceAnimator.GetBool(FreeFall));
    }
}
