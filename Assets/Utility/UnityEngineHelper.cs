using System;
using System.Collections;
using UnityEngine;

namespace Racerr.Utility
{
    public class UnityEngineHelper
    {
        /// <summary>
        /// C# doesn't support yielding inside lambda functions. Hence, everytime
        /// we want to use a coroutine we always have to make a normal function, 
        /// which is annoying.
        /// This is a trick to allow us to use lambdas with coroutines for common use cases.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehavior to run the coroutine on.</param>
        /// <param name="action">Lambda function.</param>
        public static void AsyncYieldThenExecute(MonoBehaviour monoBehaviour, YieldInstruction yieldInstruction, Action action)
        {
            monoBehaviour.StartCoroutine(YieldThenExecute(yieldInstruction, action));
        }

        static IEnumerator YieldThenExecute(YieldInstruction yieldInstruction, Action action)
        {
            yield return yieldInstruction;
            action();
        }

        public static void AsyncWaitForConditionThenExecute(MonoBehaviour monoBehaviour, Func<bool> condition, Action action)
        {
            monoBehaviour.StartCoroutine(WaitForConditionThenExecute(condition, action));
        }

        static IEnumerator WaitForConditionThenExecute(Func<bool> condition, Action action)
        {
            while (!condition())
            {
                yield return null;
            }
            action();
        }
    }
}