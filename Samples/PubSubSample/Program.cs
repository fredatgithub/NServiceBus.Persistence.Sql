﻿using System;
using NServiceBus;

class Program
{
    static void Main()
    {
        var busConfiguration = ConfigBuilder.Build("PubSub");
        using (var bus = Bus.Create(busConfiguration).Start())
        {
            var myEvent = new MyEvent
            {
                Property = "PropertyValue"
            };
            bus.Publish(myEvent);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}