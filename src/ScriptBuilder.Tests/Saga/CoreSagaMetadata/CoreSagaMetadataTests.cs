﻿using System;
using System.IO;
using Mono.Cecil;
using NServiceBus.Persistence.Sql.ScriptBuilder;
using NUnit.Framework;
using Particular.Approvals;

[TestFixture]
public partial class CoreSagaMetadataTests
{
    ModuleDefinition module;

    public CoreSagaMetadataTests()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "ScriptBuilder.Tests.dll");
        var readerParameters = new ReaderParameters(ReadingMode.Deferred);
        module = ModuleDefinition.ReadModule(path, readerParameters);
    }

    [Test]
    public void SingleMapping()
    {
        TestSagaDefinition<SingleMappingSaga>();
    }

    [Test]
    public void SingleMappingValueType()
    {
        TestSagaDefinition<SingleMappingValueTypeSaga>();
    }

    [Test]
    public void DualMapping()
    {
        TestSagaDefinition<DualMappingSaga>();
    }

    [Test]
    public void DontMapWithIntermediateBase()
    {
        // Won't map to a saga definition (result will be null) but won't throw either.
        // Behavior is the same as SqlSaga. Throw will occur at runtime.
        TestSagaDefinition<HasBaseSagaClass>();
    }

    [Test]
    public void DontAllowMethodCallInMapping()
    {
        TestSagaDefinition<MethodCallInMappingSaga>();
    }

    [Test]
    public void DontAllowPassingMapper()
    {
        TestSagaDefinition<PassingMapperSaga>();

    }

    [Test]
    public void DontMapConflictingCorrelationIds()
    {
        TestSagaDefinition<ConflictingCorrelationSaga>();
    }

    [Test]
    public void DontMapSwitchingLogic()
    {
        TestSagaDefinition<SwitchingLogicSaga>();

    }

    [Test]
    public void DontMapDelegateCalls()
    {
        TestSagaDefinition<DelegateCallingSaga>();
    }

    [Test]
    public void DontAllowForLoop()
    {
        TestSagaDefinition<ForLoopSaga>();
    }

    [Test]
    public void DontAllowWhileLoop()
    {
        TestSagaDefinition<WhileLoopSaga>();
    }

    [Test]
    public void AllowConcatenatingMsgProperties()
    {
        TestSagaDefinition<ConcatMsgPropertiesSaga>();
    }

    [Test]
    public void AllowConcatenatingMsgPropertiesWithFormat()
    {
        TestSagaDefinition<ConcatMsgPropertiesWithFormatSaga>();
    }

    [Test]
    public void AllowConcatenatingMsgPropertiesWithInterpolation()
    {
        TestSagaDefinition<ConcatMsgPropertiesWithInterpolationSaga>();
    }

    [Test]
    public void AllowConcatenatingMsgPropertiesWithOtherMethods()
    {
        TestSagaDefinition<ConcatMsgPropertiesWithOtherMethods>();
    }

    [Test]
    public void SupplyAdditionalMetadataViaAttribute()
    {
        TestSagaDefinition<MetadataInAttributeSaga>();
    }

    [Test]
    public void TestSagaInVB()
    {
        var vbPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "VBTestCode.dll");
        var vbModule = ModuleDefinition.ReadModule(vbPath, new ReaderParameters(ReadingMode.Deferred));

        TestSagaDefinition<VBTestCode.VBMultiTestSaga>(vbModule);
    }

    [Test]
    public void TestReverseMapping()
    {
        TestSagaDefinition<ReverseMappingSaga>();
    }

    [Test]
    public void TestHeaderMapping()
    {
        TestSagaDefinition<HeaderMappingSaga>();
    }

    [Test]
    public void TestReverseHeaderMapping()
    {
        TestSagaDefinition<ReverseHeaderMappingSaga>();
    }

    void TestSagaDefinition<TSagaType>(ModuleDefinition moduleToUse = null)
    {
        moduleToUse ??= module;

        var dataType = moduleToUse.GetTypeDefinition<TSagaType>();
        var instructions = InstructionAnalyzer.GetConfigureHowToFindSagaInstructions(dataType);

        var results = new SagaInspectionResults
        {
            HasUnmanagedCalls = InstructionAnalyzer.CallsUnmanagedMethods(instructions),
            HasUnexpectedCalls = InstructionAnalyzer.CallsUnexpectedMethods(instructions),
            HasBranchingLogic = InstructionAnalyzer.ContainsBranchingLogic(instructions)
        };

        try
        {
            SagaDefinitionReader.TryGetSagaDefinition(dataType, out results.SagaDefinition);
        }
        catch (Exception x)
        {
            results.Exception = x.Message;
        }

        Approver.Verify(results);
    }

    class SagaInspectionResults
    {
        public bool HasUnmanagedCalls;
        public bool HasUnexpectedCalls;
        public bool HasBranchingLogic;
        public SagaDefinition SagaDefinition;
        public string Exception;
    }




}