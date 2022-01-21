using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class BendingManager : MonoBehaviour
{
    [Range(-3, 3)]
    [SerializeField] public float m_bendAmountUpDown = 1f;
    [Range(-5, 5)]
    [SerializeField] public float m_bendAmountSide = 1f;

    private int m_bendAmountUpDownId;
    private int m_bendAmountSideId;

    #region Constants

    private const string BENDING_FEATURE = "ENABLE_BENDING";

    #endregion

    private void Awake()
    {
        //Disable shader in edit mode
        if(Application.isPlaying)
            Shader.EnableKeyword(BENDING_FEATURE);
        else
            Shader.DisableKeyword(BENDING_FEATURE);
    }
    private void Start()
    {
        m_bendAmountUpDownId = Shader.PropertyToID("_bendAmountUpDown");
        m_bendAmountSideId = Shader.PropertyToID("_bendAmountSide");

        StartCoroutine("BendChange");
    }
    void Update()
    {
        Shader.SetGlobalFloat(m_bendAmountUpDownId, m_bendAmountUpDown);
        Shader.SetGlobalFloat(m_bendAmountSideId, m_bendAmountSide);
    }

    //Linear interpolation for smooth change of the bending amount of the shader.
    IEnumerator BendChange()
    {
        while(true)
        {
            float elapsedTime = 0;
            float waitTime = Random.Range(5, 15);

            float nextBendAmountUpDown = Random.Range(-3, -1);
            float nextBendAmountSide = Random.Range(-3, 3);

            float currentBendAmountUpDown = m_bendAmountUpDown;
            float currentBendAmountSide = m_bendAmountSide;

            while (elapsedTime < waitTime)
            {
                m_bendAmountUpDown = Mathf.Lerp(currentBendAmountUpDown, nextBendAmountUpDown, (elapsedTime / waitTime));
                m_bendAmountSide = Mathf.Lerp(currentBendAmountSide, nextBendAmountSide, (elapsedTime / waitTime));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            yield return null;
        }
    }
}
