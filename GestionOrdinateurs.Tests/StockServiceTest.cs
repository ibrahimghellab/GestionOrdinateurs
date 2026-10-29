using FluentAssertions;
using GestionOrdinateurs.API.Services;
using Xunit;

namespace GestionOrdinateurs.Tests;

public class StockServiceTest
{
    private StockService _service;

    public StockServiceTest()
    {
        _service = new StockService();
    }


    [Fact]
    public void StockSSD_ParDefaut_Vaut10()
    {
        _service.StockSSD.Should().Be(10);
    }

    [Fact]
    public void ConsommerSSD_UneFois_DiminueLeStockDe1()
    {
        _service.ConsommerSSD();

        _service.StockSSD.Should().Be(9);
    }

    [Fact]
    public void ConsommerSSD_DixFoisEnParallele_StockAZero()
    {
        //Même appel que l'endpoint GetStockSSD : sans le lock, le résultat varie d'une exécution à l'autre
        Parallel.For(
            0,
            10,
            i => _service.ConsommerSSD());

        _service.StockSSD.Should().Be(0);
    }
}
