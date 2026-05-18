using UniftUI;
using UnityEngine;

public sealed class CounterView : UniftView
{
    private readonly State<int> count = new State<int>(0);

    private void Start()
    {
        VStack(() =>
        {
            Text(() => $"Count: {count.Value}", new State[] { count })
                .fontSize(28)
                .bold();

            Button("Increment", () => count.Value++)
                .padding(12)
                .background(new Color(0.15f, 0.38f, 0.9f))
                .foregroundColor(Color.white)
                .cornerRadius(10);
        }, spacing: 12f)
        .padding(24)
        .Build(GetComponent<Canvas>());
    }
}