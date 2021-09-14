using Clicker.Models;
using UnityEngine;

namespace Clicker.DetachedScrypts
{
    class TestDestroyer : MonoBehaviour
    {
        private void Start()
        {
            if (!Values.data.isTest)
                Destroy(gameObject);
        }
    }
}
