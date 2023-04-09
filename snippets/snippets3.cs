void Layout(List<Rectangle> rectangles, List<Rectangle> bounds)
{
    const int MaxIterations = 10000;
    const int Padding = 10;
    const float SpringConstant = 0.01f;
    const float ElectricalConstant = 1000f;

    // Initialize the rectangles' positions randomly
    Random rand = new Random();
    foreach (var rect in rectangles)
    {
        rect.Left = rand.Next(Padding, bounds[0].Width - Padding - rect.Width);
        rect.Top = rand.Next(Padding, bounds[0].Height - Padding - rect.Height);
    }

    // Calculate the initial forces on each rectangle
    for (int iteration = 0; iteration < MaxIterations; iteration++)
    {
        // Calculate the spring forces on each rectangle
        foreach (var rect in rectangles)
        {
            foreach (var connector in rect.Connectors)
            {
                Rectangle destRect = connector.Destination;
                LineSegment line = new LineSegment
                {
                    BeginX = rect.Left + rect.Width / 2,
                    BeginY = rect.Top + rect.Height / 2,
                    EndX = destRect.Left + destRect.Width / 2,
                    EndY = destRect.Top + destRect.Height / 2,
                };
                float distance = (float)Math.Sqrt(Math.Pow(line.EndX - line.BeginX, 2) + Math.Pow(line.EndY - line.BeginY, 2));
                float force = SpringConstant * (distance - ElectricalConstant) / distance;

                // Apply the force to the rectangles
                float deltaX = force * (line.EndX - line.BeginX);
                float deltaY = force * (line.EndY - line.BeginY);
                rect.Left += (int)deltaX;
                rect.Top += (int)deltaY;
                destRect.Left -= (int)deltaX;
                destRect.Top -= (int)deltaY;

                // Add the line segment to the connector
                connector.Segments.Clear();
                connector.Segments.Add(line);
            }
        }

        // Calculate the electrical forces on each rectangle
        for (int i = 0; i < rectangles.Count; i++)
        {
            for (int j = i + 1; j < rectangles.Count; j++)
            {
                Rectangle rect1 = rectangles[i];
                Rectangle rect2 = rectangles[j];
                float deltaX = rect1.Left + rect1.Width / 2 - (rect2.Left + rect2.Width / 2);
                float deltaY = rect1.Top + rect1.Height / 2 - (rect2.Top + rect2.Height / 2);
                float distance = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
                float force = ElectricalConstant / (distance * distance);

                // Apply the force to the rectangles
                deltaX = force * deltaX;
                deltaY = force * deltaY;
                rect1.Left -= (int)deltaX;
                rect1.Top -= (int)deltaY;
                rect2.Left += (int)deltaX;
                rect2.Top += (int)deltaY;
            }
        }
    }
}
