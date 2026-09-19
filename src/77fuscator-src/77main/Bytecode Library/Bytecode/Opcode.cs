namespace geniussolution.Bytecode_Library.Bytecode
{
    public enum Opcode
    {
        Move,
        LoadConst,
        LoadBool,
        LoadNil,
        GetUpval,
        GetGlobal,
        GetTable,
        SetGlobal,
        SetUpval,
        SetTable,
        NewTable,
        Self,
        Add,
        Sub,
        Mul,
        Div,
        Mod,
        Pow,
        Unm,

        Not,
        Len,
        Concat,
        Jmp,
        Eq,
        Lt,
        Le,
        Test,
        TestSet,
        Call,
        TailCall,
        Return,
        ForLoop,
        ForPrep,
        TForLoop,
        SetList,
        Close,
        Closure,
        VarArg,

        // BITWISE (Luau extras) - MUST stay after VarArg so that enum values
        // 0..37 line up with the stock Lua 5.1 opcode table emitted by
        // LuaCompiler-O.dll. Inserting them mid-table shifts Jmp/Call/Return/...
        // by 5 and makes the Deserializer decode garbage on opcode >= 22.
        BOR,
        BAND,
        BXOR,
        BLSHFT,
        BRSHFT,
        BNOT,
        INTDIV,
        // BITWISE END

        //Custom VM opcodes
        SetTop,

        PushStack,
        PushStackPersistent,
        NewStack,
        SetFenv,
        GetFEnv,

        PushCache,
        PushConstCache,
        PushInsts,
        PushXor,
        PushConsts,

        PrepInsts,
        PrepConsts,
        PrepProtos,
        PrepParams,
        PrepReturnWrapper,
        PrepInstrPoint,
        PrepTop,
        PrepVarargs,
        PrepArgs,
        PrepPCount,
        PrepLUpvals,
        PrepStack,
        PrepStackArgs,
        PrepVarargSize,
        PrepEnv,
        PrepVararg,

        LoadN,
        GetTableN,
        CallSpec,
        SetTableNB,
        SetTableNC,
        SetTableNBC,

        NOP,

        GetInstruction,
        PushEnumKey,
        PushAKey,
        PushBKey,
        PushCKey,
        PushXorKey,

        Polymorphic,

        FuncDecl1,
        FuncDecl2,
        FuncDecl3,

        LoadConstList,
        MoveList,
        Replay,
        ReplayJmp,

        UnpackTable,
        CopyTable,

        ReverseConstBInt,
        ReverseConstCInt,
        ReverseStkBInt,
        ReverseStkCInt,
        Dynamic
    }
}