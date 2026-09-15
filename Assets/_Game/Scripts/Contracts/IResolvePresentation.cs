using System;
using System.Collections.Generic;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Contracts
{
    /// <summary>
    /// Visual/FX presentation for a resolved placement.
    /// Runtime passes only neutral UnityEngine.Object handles.
    /// </summary>
    public interface IResolvePresentation
    {
        void SetRuntimeResolveOwnership(bool enabled);

        void PresentResolve(
            UnityEngine.Object shapeHandle,
            IReadOnlyList<IReadOnlyList<UnityEngine.Object>> lines,
            int scoreGain,
            int combo,
            ResolveScoreFeedback feedback,
            Action completed);
    }
}
