using System;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Builders;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration
{
    internal record EntityTestEntry(Type Type, ITestEntityTypeBuilder Builder, object Configuratin);
}
