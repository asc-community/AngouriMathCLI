//  amcli - command line interface for CAS AngouriMath
//  Copyright (C) 2022 Angouri
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

using AngouriMath;
using AngouriMath.Extensions;
using HonkSharp;
using HonkSharp.Functional;
using PeterO.Numbers;

var cliArgs = System.Environment.GetCommandLineArgs();
var reader = new ArgReader(cliArgs);

var prec = GetEnv<int>("AMCLI_PRECISION", 100);
MathS.Settings.DecimalPrecisionContext.Set(new EContext(prec, ERounding.HalfUp, -prec, 10 * prec, false));
Entity expr;
Entity.Variable v;
string res;

var cmd = reader.Next();
switch (cmd)
{
    case "help" or "-h" or "--help":
        Console.WriteLine("""

                  .d8b.    .88b  d88.    .o88b.   db        d888888b 
                 d8' `8b   88'YbdP`88   d8P  Y8   88          `88'   
                 88ooo88   88  88  88   8P        88           88    
                 88~~~88   88  88  88   8b        88           88    
                 88   88   88  88  88   Y8b  d8   88booo.     .88.   
                 YP   YP   YP  YP  YP    `Y88P'   Y88888P   Y888888P 
                                            
        amcli (c) 2022 Angouri
        This is free software. You're free to use, modify and redistribute it.
        GNU GPL v3 license. Made with AngouriMath (MIT Expat) and .NET (MIT Expat).

        Why use it?
        - Free and cross-platform
        - CLI interface for script automations
        - Piping for complex operations
        - Fast and small
        
        COMMANDS

            EVAL

            amcli eval - to evaluate into a single number, boolean, or a+bi format 
            for complex numbers. Expects one argument.

            Example:
                $ amcli eval "1 / 2"
                0.5
                $ amcli eval "e ^ pi > pi ^ e"
                true

            DIFF

            amcli diff - to differentiate the expression over the given variable
            (the first argument). Expects two arguments.

            Example:
                $ amcli diff "x" "sin(x)"
                cos(x)
                $ amcli diff "x" "1 + x^2"
                2 * x
                $ echo "1 + x^2" | amcli diff "x"
                2 * x

            SIMP

            amcli simp - to simplify the expression. Expects one argument.

            Example:
                $ amcli simp "sin(x)^2 + cos(x)^2"
                1

            FSIMP

            amcli fsimp - to simplify the expression "faster". This one works
            closer to eval than to simp, but unlike eval, it won't try to
            collapse to a single number or boolean (e. g. sqrt(3) will stay as
            it is). Expects one argument. 

            Example:
                $ amcli fsimp "sin(x)^2 + cos(x)^2"
                1

            SOLVE

            amcli solve - to solve a *statement* over the given variable. A 
            *statement* is an expression, otherwise evaluable to true or false
            (e. g. "x > 3" is a statement, but "x ^ 2" is not).

            When the solution set is a finite solution, all solutions are written 
            line-by-line. Otherwise, it's written as one line.

            Example:
                $ amcli solve "x" "x2 - 1 = 0"
                1
                -1
                $ amcli solve x "x2 > 1"
                (-oo; -1) \/ (1; +oo)

            LATEX

            amcli latex - to convert an expression into LaTeX format. Expects one
            argument.

            Example:
                $ amcli latex "1/2"
                \frac{1}{2}
                $ amcli latex "(sqrt(3) + x) / limit(sin(x) / x, x, 0)"
                \frac{\sqrt{3}+x}{\lim_{x\to 0} \left[\frac{\sin\left(x\right)}{x}\right]}

            SUB

            amcli sub - to substitute an expression instead of a variable. Expects
            three arguments (variable to substitute, expression to be substituted
            instead of the variable, expression).

            Example:
                $ amcli sub x pi "sin(x / 3)"
                sin(pi / 3)
                $ amcli sub x "pi / 3" "sin(x)"
                sin(pi / 3)

        PIPING
        
            Any argument can be received either as a CLI argument or through
            standard input. For example,

                amcli eval "1 + 1"
            
            is equivalent to

                echo "1 + 1" | amcli eval

            This allows to pipe complex evaluations:

                echo "sin(x) * cos(y)" \     # 0. initial expression
                | amcli diff x \             # 1. differentiate over x
                | amcli sub x y \            # 2. substitute y instead of x
                | amcli diff y \             # 3. differentiate over y
                | amcli sub y "pi/3" \       # 4. substitute pi/3 instead of y
                | amcli simplify             # 5. simplify

            Prints

                -1/2 * sqrt(3)

            Special symbol "_" (underscore) can be used to use stdinput instead
            of an argument. For instance, if you want to substitute the result of
            an operation into another expression:
            
                echo "e^x" \
                | amcli sub u _ "u / (1 + u)" \
                | amcli sub x 10 \
                | amcli eval

            Here the result of `echo` is substituted instead of the second argument
            of `amcli sub`, not the last one.

        SETTINGS
            
            All settings are set using environment variables.

            AMCLI_PRECISION - precision/number of digits in decimal numbers.
            Default - 100.

        OTHER
            
            You can bind amcli to @ using aliases. On Unix-like operating systems,
            add
                
                alias @=amcli

            (or specify the full path)
        """);
        break;

    case "eval":
        expr = reader.Next().ToEntity().Evaled;
        res = expr.ToString();
        if (expr is Entity.Number.Rational rat)
            res = rat.RealPart.EDecimal.ToString();
        Console.WriteLine(res);
        break;

    case "diff":
        v = (Entity.Variable)reader.Next();
        expr = reader.Next().ToEntity();
        Console.WriteLine(expr.Differentiate(v));
        break;

    case "solve":
        v = (Entity.Variable)reader.Next();
        expr = reader.Next().ToEntity();
        var sols = expr.Solve(v);
        if (sols is Entity.Set.FiniteSet fs)
            foreach (var sol in fs)
                Console.WriteLine(sol);
        else
            Console.WriteLine(sols);
        break;

    case "latex":
        expr = reader.Next().ToEntity();
        Console.WriteLine(expr.Latexise());
        break;

    case "simp":
        expr = reader.Next().ToEntity();
        Console.WriteLine(expr.Simplify());
        break;

    case "fsimp":
        expr = reader.Next().ToEntity();
        Console.WriteLine(expr.InnerSimplified);
        break;

    case "sub":
        v = (Entity.Variable)reader.Next();
        var withWhat = reader.Next().ToEntity();
        expr = reader.Next();
        Console.WriteLine(expr.Substitute(v, withWhat));
        break;

    case "info":
        expr = reader.Next();
        var splitter = new string('-', 20);
        Console.WriteLine($"expr: {expr}");
        Console.WriteLine(splitter);
        Console.WriteLine($"vars: {expr.Vars.ToLList()}");
        Console.WriteLine(splitter);
        var vars = expr.Vars.ToArray();
        foreach (var vx in vars)
        {
            Console.Write($"diff over {vx}: ");
            Console.WriteLine(expr.Differentiate(vx).InnerSimplified);
            Console.WriteLine(splitter);
        }
        foreach (var vx in vars)
        {
            Console.Write($"roots over {vx}: ");
            Console.WriteLine(expr.Solve(vx).InnerSimplified);
            Console.WriteLine(splitter);
        }

        var diffs = new List<Entity>();
        foreach (var vx in vars)
            diffs.add(expr.Differentiate(vx).InnerSimplified);

        var system = MathS.Equations(diff);
        var sols = system.Solve(expr.Vars);
        if (sols is null)
        {
            Console.WriteLine("amcli wasn't able to find any extremas");
            break;
        }

        var diffMatrix = new Entity.Matrix(dims => 
            expr
            .Differentiate(vars[dims[0]])
            .Differentiate(vars[dims[1]])
            .InnerSimplified,
            vars.Length,
            vars.Length);

        foreach (var (i, sol) in sols.Enumerate())
        {
            var exprToSub = expr;
            var diffMatToSub = diffMatrix;
            foreach (var (vr, val) in sol.Zip(vars))
            {
                exprToSub = exprToSub.Substitute(vr, val);
                diffMatToSub = diffMatToSub.Substitute(vr, val);
            }
            var fValue = exprToSub.Evaled;
            var det = diffMatToSub.Evaled.Determinant.EvalNumerical();
            Console.WriteLine($"Extrema #{i}:");
            Console.WriteLine($"Point: {sol}");
            Console.WriteLine($"Value: {fValue}");
            Console.WriteLine($"Hessian det: {det}");
        }

        break;

    default:
        Console.WriteLine($"Unrecognized command `{cmd}`");
        break;
}

static T GetEnv<T>(string name, T def)
{
    if (Environment.GetEnvironmentVariable(name) is not string value)
        return def;
    if (typeof(T) == typeof(int))
        return (T)(object)int.Parse(value);
    if (typeof(T) == typeof(string))
        return (T)(object)value;
    throw new();
}

public sealed class ArgReader
{
    private readonly string[] args;
    private int curr = 1;
    public ArgReader(string[] args)
        => this.args = args;
    public string Next()
    {
        if (curr < args.Length)
        {
            var res = args[curr];
            if (res is "_")
                res = Console.ReadLine()!;
            curr++;
            return res;
        }
        return Console.ReadLine()!;
    }
}

