﻿using Plang.Compiler.Backend.Prt;
using Plang.Compiler.Backend.Coyote;
using Plang.Compiler.Backend.Uclid5;
using System.Collections.Generic;

namespace Plang.Compiler.Backend
{
    public static class TargetLanguage
    {
        private static readonly IDictionary<CompilerOutput, ICodeGenerator> BackendMap =
            new Dictionary<CompilerOutput, ICodeGenerator>();

        static TargetLanguage()
        {
            RegisterCodeGenerator(CompilerOutput.Coyote, new CoyoteCodeGenerator());
            RegisterCodeGenerator(CompilerOutput.C, new PrtCodeGenerator());
            RegisterCodeGenerator(CompilerOutput.Uclid5, new Uclid5CodeGenerator());
        }

        private static void RegisterCodeGenerator(CompilerOutput name, ICodeGenerator generator)
        {
            BackendMap[name] = generator;
        }

        public static ICodeGenerator GetCodeGenerator(CompilerOutput languageName)
        {
            return BackendMap[languageName];
        }
    }
}