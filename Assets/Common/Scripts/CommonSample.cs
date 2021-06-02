using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonSample : MonoBehaviour
{
    Stack<IEnumerator> numStack = new Stack<IEnumerator>();

    public class NumStorage
    {
        public int num;
    }

    private NumStorage numStorage;

    public void BTN_Test()
    {
        numStorage = new NumStorage() { num = 5 };

        numStack.Push(Cor_Test(numStorage));
        numStack.Push(Cor_Test(numStorage));
        numStack.Push(Cor_Test(numStorage));
        numStack.Push(Cor_Test(numStorage));

        StartCoroutine(Cor_Pop());
    }

    public IEnumerator Cor_Pop()
    {
        while (numStack.Count > 0)
        {
            yield return numStack.Pop();

        }

    }

    private IEnumerator Cor_Test(NumStorage storage)
    {
        Debug.Log(storage.num);

        storage.num *= 2;

        yield return new WaitForSeconds(1f);
    }
}
