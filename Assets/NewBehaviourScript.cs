using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    public int index;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        index = GetComponent<EndlessScrollLayout>().index;
    }

    public void OnIndexChanged(int index, RectTransform transform)
    {
        transform.gameObject.name = index.ToString();
        transform.GetComponentInChildren<Text>().text = index.ToString();
    }

    [ContextMenu("SetIndex")]
    public void SetIndexRand()
    {
        EndlessScrollLayout bsv = GetComponent<EndlessScrollLayout>();
        int index = Random.Range(0, bsv.ObjectCount-1);
        Debug.Log(index);
        bsv.index = index;
    }

    [ContextMenu("SetMaxIndex")]
    public void SetMaxIndex()
    {
        EndlessScrollLayout bsv = GetComponent<EndlessScrollLayout>();
        bsv.index = bsv.ObjectCount - 1;
    }
}
