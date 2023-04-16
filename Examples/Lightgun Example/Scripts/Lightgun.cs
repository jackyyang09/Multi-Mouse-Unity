using UnityEngine;
using UnityEngine.UI;

namespace MultiMouseUnity.Example
{
    public class Lightgun : MonoBehaviour
    {
        [SerializeField] Transform uiTransform;
        [SerializeField] Image highlight;
        [SerializeField] Crosshair crosshair;

        Camera cam;

        public System.Action OnShoot;

        // Start is called before the first frame update
        void Start()
        {
            cam = Camera.main;
            highlight.CrossFadeAlpha(0, 0, false);
        }

        public void Shoot(WeaponObject weapon)
        {
            if (weapon.pellets == 0) // This weapon is not a shotgun
            {
                FireShotAt(crosshair.transform.position, weapon);
            }
            else // This weapon is a shotgun
            {
                for (int i = 0; i < weapon.pellets; i++)
                {
                    var offset = (Vector3)Random.insideUnitCircle * weapon.bulletSpread;
                    FireShotAt(crosshair.transform.position + offset, weapon);
                }
            }
        }

        void FireShotAt(Vector2 pos, WeaponObject weapon)
        {
            Ray ray = cam.ScreenPointToRay(pos);
            RaycastHit hit;
            IShootable shootable;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.TryGetComponent(out shootable))
                {
                    shootable.InvokeOnShotBehaviour();

                    highlight.CrossFadeAlpha(1, 0, false);
                    highlight.CrossFadeAlpha(0, 0.5f, false);
                }
            }

            // Instantiating and destroying muzzle flash effects can be computationally expensive!
            // In a real game project, you should consider other methods of creating these effects
            var muzzleFlash = Instantiate(weapon.muzzleFlashPrefab, uiTransform);
            muzzleFlash.transform.position = pos;
            Destroy(muzzleFlash, 1);

            OnShoot?.Invoke();
        }
    }
}