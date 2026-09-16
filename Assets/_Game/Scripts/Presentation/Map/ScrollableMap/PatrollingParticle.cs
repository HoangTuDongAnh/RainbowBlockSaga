using System.Collections;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Map.ScrollableMap
{
    public class PatrollingParticle : MonoBehaviour
    {
        public ParticleSystem fireflySystem;
        public float boundary = 5f; // adjust boundary to your desired area
        public float checkInterval = 0.2f; // interval to check particle positions
        public float strengthOut = 0.2f; // strength when outside boundary
        public float strengthIn = 1f; // strength when inside boundary

        private ParticleSystem.NoiseModule noiseModule;

        private void Start()
        {
            noiseModule = fireflySystem.noise;
            StartCoroutine(CheckParticlePositions());
        }

        private IEnumerator CheckParticlePositions()
        {
            while (true)
            {
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[fireflySystem.particleCount]; 
                fireflySystem.GetParticles(particles);

                for (int i = 0; i < particles.Length; i++)
                {
                    Vector3 directionToCenter = (Vector3.zero - particles[i].position).normalized;
                    float distanceToCenter = Vector3.Distance(Vector3.zero, particles[i].position);

                    if (distanceToCenter > boundary)
                    {
                        particles[i].velocity += directionToCenter * strengthOut;
                    }
                    else
                    {
                        particles[i].velocity += directionToCenter * strengthIn;
                    }
                }

                fireflySystem.SetParticles(particles, particles.Length);

                yield return new WaitForSeconds(checkInterval);
            }
        }
    }
}