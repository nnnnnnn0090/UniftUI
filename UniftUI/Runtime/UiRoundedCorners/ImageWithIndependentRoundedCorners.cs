using UnityEngine;
using UnityEngine.UI;

namespace Nobi.UiRoundedCorners {
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class ImageWithIndependentRoundedCorners : MonoBehaviour {
		private static readonly int prop_halfSize = Shader.PropertyToID("_halfSize");
		private static readonly int prop_radiuses = Shader.PropertyToID("_r");
		private static readonly int prop_rect2props = Shader.PropertyToID("_rect2props");
		private static readonly int prop_OuterUV = Shader.PropertyToID("_OuterUV");

		private static readonly Vector2 wNorm = new Vector2(.7071068f, -.7071068f);
		private static readonly Vector2 hNorm = new Vector2(.7071068f, .7071068f);

		[SerializeField] private Vector4 _r = new Vector4(40f, 40f, 40f, 40f);
		public Vector4 r {
			get { return _r; }
			set {
				_r = value;
				if (material != null) {
					Refresh();
				}
			}
		}

		private Material material;
		private Vector4 outerUV = new Vector4(0, 0, 1, 1);

		[HideInInspector, SerializeField] private Vector4 rect2props;
		[HideInInspector, SerializeField] private MaskableGraphic image;

		private void OnValidate() {
			Validate();
			Refresh();
		}

		private void OnEnable() {
			var other = GetComponent<ImageWithRoundedCorners>();
			if (other != null) {
				r = Vector4.one * other.radius;
				DestroyHelper.Destroy(other);
			}

			Validate();
			Refresh();
		}

		private void OnRectTransformDimensionsChange() {
			if (enabled && material != null) {
				Refresh();
			}
		}

		private void OnDestroy() {
			if (image != null) {
				image.material = null;
			}

			DestroyHelper.Destroy(material);
			image = null;
			material = null;
		}

		public void Validate() {
			if (material == null) {
				material = new Material(Shader.Find("UI/RoundedCorners/IndependentRoundedCorners"));
			}

			if (image == null) {
				TryGetComponent(out image);
			}

			if (image != null) {
				image.material = material;
			}

			if (image is Image uiImage && uiImage.sprite != null) {
				outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(uiImage.sprite);
			}
		}

		public void Refresh() {
			var rect = ((RectTransform)transform).rect;
			RecalculateProps(rect.size);
			material.SetVector(prop_rect2props, rect2props);
			material.SetVector(prop_halfSize, rect.size * .5f);
			material.SetVector(prop_radiuses, _r);
			material.SetVector(prop_OuterUV, outerUV);
		}

		private void RecalculateProps(Vector2 size) {
			var aVec = new Vector2(size.x, -size.y + r.x + r.z);
			var halfWidth = Vector2.Dot(aVec, wNorm) * .5f;
			rect2props.z = halfWidth;

			var bVec = new Vector2(size.x, size.y - r.w - r.y);
			var halfHeight = Vector2.Dot(bVec, hNorm) * .5f;
			rect2props.w = halfHeight;

			var efVec = new Vector2(size.x - r.x - r.y, 0);
			var egVec = hNorm * Vector2.Dot(efVec, hNorm);
			var ePoint = new Vector2(r.x - (size.x / 2), size.y / 2);
			var origin = ePoint + egVec + wNorm * halfWidth + hNorm * -halfHeight;
			rect2props.x = origin.x;
			rect2props.y = origin.y;
		}
	}
}
