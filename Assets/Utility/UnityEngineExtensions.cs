using System;
using System.Collections;
using UnityEngine;

namespace Racerr.Utility
{
    public static class UnityEngineExtensions
    {
        /// <summary>
        /// This is a trick to allow us to use lambdas after a single yield instruction.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehavior to run the coroutine on.</param>
        /// <param name="yieldInstruction">The yield instruction.</param>
        /// <param name="action">Lambda function.</param>
        public static void YieldThenExecuteAsync(this MonoBehaviour monoBehaviour, YieldInstruction yieldInstruction, Action action)
        {
            monoBehaviour.StartCoroutine(YieldThenExecute(yieldInstruction, action));
        }

        /// <summary>
        /// This is a trick to allow us to use lambdas after a single yield instruction.
        /// Pass into StartCoroutine().
        /// </summary>
        /// <param name="yieldInstruction">The yield instruction.</param>
        /// <param name="action">Lambda function.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        static IEnumerator YieldThenExecute(YieldInstruction yieldInstruction, Action action)
        {
            yield return yieldInstruction;
            action();
        }

        /// <summary>
        /// This is a trick to allow us to asynchronously wait for a condition to be true then execute a lambda.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehavior to run the coroutine on.</param>
        /// <param name="condition">The boolean function which we wait to return true.</param>
        /// <param name="action">Lambda function.</param>
        public static void WaitForConditionThenExecuteAsync(this MonoBehaviour monoBehaviour, Func<bool> condition, Action action)
        {
            monoBehaviour.StartCoroutine(WaitForConditionThenExecute(condition, action));
        }

        /// <summary>
        /// This is a trick to allow us to asynchronously wait for a condition to be true then execute a lambda.
        /// Pass into StartCoroutine().
        /// </summary>
        /// <param name="condition">The boolean function which we wait to return true.</param>
        /// <param name="action">Lambda function.</param>
        /// <returns></returns>
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