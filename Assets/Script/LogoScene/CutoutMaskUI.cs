using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CutoutMaskUI : Image
{
    /* https://dhshin94.tistory.com/186 참고 */
    private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");

    // 매번 new 하지 않고, 유니티가 UI를 그릴 때 사용할 재질을 직접 수정해 넘긴다.
    public override Material materialForRendering
    {
        get
        {
            // 부모(Image)의 기본 재질을 가져온다.
            // base 가 유니티의 Image 컴포넌트
            Material mat = base.materialForRendering;

            if (mat != null)
            {
                // 마스크(Knob 이미지)가 없는 부분만 검게 칠하라는 명령.
                mat.SetInt(StencilComp, (int)CompareFunction.NotEqual);
            }

            return mat;
        }
    }

    // 씬 전환 시 렌더링이 씹히는 걸 방지하기 위해 강제로 갱신한다.
    /* https://do-workspace.tistory.com/58 참고 */
    protected override void OnEnable()
    {
        base.OnEnable();
        SetMaterialDirty(); // Material 다시 계산하라고 유니티 엔진에 호출.
    }
}