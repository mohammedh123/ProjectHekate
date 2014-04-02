using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Irony.Ast;
using Irony.Parsing;
using Irony.Interpreter;
using Irony.Interpreter.Ast;

namespace ProjectHekate.Scripting
{
    [Language("BulletGrammar", "1.0", "A grammar for describing bullet patterns")]
    public class BulletGrammar : Grammar
    {
        TerminalSet _skipTokensInPreview = new TerminalSet();

        public BulletGrammar()
        {
            // lexical structure
            var stringLiteral = TerminalFactory.CreateCSharpString("StringLiteral");
            var charLiteral = TerminalFactory.CreateCSharpChar("CharLiteral");
            var number = TerminalFactory.CreateCSharpNumber("Number");
            var identifier = TerminalFactory.CreateCSharpIdentifier("Identifier");

            var singleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            NonGrammarTerminals.Add(singleLineComment);

            // symbols
            var semi = ToTerm(";", "semicolon");
            var dot = ToTerm(".", "dot");
            var comma = ToTerm(",", "comma");
            var commasOpt = new NonTerminal("commasOpt");
            commasOpt.Rule = MakeStarRule(commasOpt, null, comma);
            var lbr = ToTerm("{");
            var rbr = ToTerm("}");
            var lpar = ToTerm("(");
            var rpar = ToTerm(")");
            
            // expressions
            var argument = new NonTerminal("argument");
            var argumentList = new NonTerminal("argumentList");
            var argumentListOpt = new NonTerminal("argumentListOpt");
            var argumentListPar = new NonTerminal("argumentListPar");
            var argumentListParOpt = new NonTerminal("argumentListParOpt");
            var expression = new NonTerminal("expression", "expression");
            var expressionList = new NonTerminal("expressionList", "expressionList");
            var expressionOpt = new NonTerminal("expressionOpt");
            var unaryOperator = new NonTerminal("unaryOperator");
            var binOperator = new NonTerminal("binOp", "operator symbol");
            var assignmentOperator = new NonTerminal("assignmentOperator");
            var literal = new NonTerminal("literal", typeof(LiteralValueNode));
            var incrOrDecr = new NonTerminal("incrOrDecr");
            var incrOrDecrOpt = new NonTerminal("incrOrDecrOpt");
            var memberAccess = new NonTerminal("memberAccess", typeof(MemberAccessNode));

            var parenthesizedExpression = new NonTerminal("parenthesizedExpression");
            var unaryExpression = new NonTerminal("unaryExpression", typeof(UnaryOperationNode));
            var preIncrDecrExpression = new NonTerminal("preIncrDecrExpression", typeof(IncDecNode));
            var postIncrDecrExpression = new NonTerminal("postIncrDecrExpression", typeof(IncDecNode));
            var primaryExpression = new NonTerminal("primaryExpression");
            var binOpExpression = new NonTerminal("binOpExpression", typeof(BinaryOperationNode));
            
            // statements
            var statement = new NonTerminal("statement", "statement");
            var statementList = new NonTerminal("statementList", typeof(StatementListNode));
            var statementListOpt = new NonTerminal("statementListOpt");
            var declarationStatement = new NonTerminal("declarationStatement");
            var embeddedStatement = new NonTerminal("embeddedStatement");
            var iterationStatement = new NonTerminal("iterationStatement");
            var assignmentStatement = new NonTerminal("assignmentStatement", typeof(AssignmentNode));
            var statementExpression = new NonTerminal("statementExpression");
            var statementExpressionList = new NonTerminal("statementExpressionList");
            var localVariableDeclaration = new NonTerminal("localVariableDeclaration");
            var functionCallStatement = new NonTerminal("functionCallExpression");
            var initializeDeclaration = new NonTerminal("initializeDeclaration");
            var block = new NonTerminal("block", typeof(StatementListNode));
            var ifStatement = new NonTerminal("ifStatement", typeof(IfNode));
            var elseClauseOpt = new NonTerminal("elseClauseOpt");
            var whileStatement = new NonTerminal("whileStatement");
            var breakStatement = new NonTerminal("breakStatement");

            // top-level objects and stuff
            var emitterDeclarationList = new NonTerminal("emitterDeclarationList");
            var emitterDeclaration = new NonTerminal("emitterDeclaration");
            var emitterBody = new NonTerminal("emitterBody");
            var emitterMemberDeclaration = new NonTerminal("emitterMemberDeclaration");
            var emitterMemberDeclarations = new NonTerminal("emitterMemberDeclarations");
            var stateDeclaration = new NonTerminal("stateDeclaration");
            var stateDeclarations = new NonTerminal("stateDeclarations");
            var stateBody = new NonTerminal("stateBody");

            // emitter stuff
            var memberDeclaration = new NonTerminal("memberDeclaration");
            var memberDeclarationsOpt = new NonTerminal("memberDeclarationsOpt");
            var fieldDeclaration = new NonTerminal("fieldDeclaration", typeof(FieldAstNode));
            var functionDeclaration = new NonTerminal("functionDeclaration");
            var functionBody = new NonTerminal("functionBody");
            var formalParameterList = new NonTerminal("formalParameterList", typeof(ParamListNode));
            var formalParameterListPar = new NonTerminal("formalParameterListPar");

            // operators
            RegisterOperators(1, "||");
            RegisterOperators(2, "&&");
            RegisterOperators(3, "|");
            RegisterOperators(4, "^");
            RegisterOperators(5, "&");
            RegisterOperators(6, "==", "!=");
            RegisterOperators(7, "<", ">", "<=", ">=");
            RegisterOperators(8, "<<", ">>");
            RegisterOperators(9, "+", "-");
            RegisterOperators(10, "*", "/", "%");
            RegisterOperators(-3, "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=");
            RegisterOperators(-2, "?");

            MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":");

            MarkTransient(emitterMemberDeclaration, memberDeclaration, statement, embeddedStatement, expression, literal, binOperator,
                primaryExpression, statementListOpt, argumentListPar, argumentListOpt, argument, statementExpression, memberAccess, functionBody, incrOrDecr, declarationStatement, iterationStatement, parenthesizedExpression);

            AddTermsReportGroup("assignment", "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=", ">>=");
            AddTermsReportGroup("typename", "bool", "float", "string", "int", "char");
            AddTermsReportGroup("statement", "if", "while", "for", "continue", "return", "break");
            AddTermsReportGroup("type declaration", "emitter", "state");
            AddTermsReportGroup("constant", number, stringLiteral, charLiteral);
            AddTermsReportGroup("constant", "true", "false", "null");
            AddTermsReportGroup("initialize", "Initialize");
            AddTermsReportGroup("unary operator", "+", "-", "!", "~");

            AddToNoReportGroup(comma, semi);
            AddToNoReportGroup("const", "++", "--", "this", "{", "}", "[");
            
            // expressions
            expression.Rule = binOpExpression | primaryExpression;
            expressionOpt.Rule = Empty | expression;
            expressionList.Rule = MakePlusRule(expressionList, comma, expression);

            argument.Rule = expression;
            argumentList.Rule = MakePlusRule(argumentList, comma, argument);
            argumentListOpt.Rule = Empty | argumentList;

            unaryOperator.Rule = ToTerm("+") | "-" | "!" | "~" | "*";
            assignmentOperator.Rule = ToTerm("=") | "+=" | "-=" | "*=" | "/=" | "%=" | "&=" | "|=" | "^=" | "<<=" | ">>=";

            binOpExpression.Rule = expression + binOperator + expression;

            unaryExpression.Rule = unaryOperator + primaryExpression;
            primaryExpression.Rule =
                literal
                | unaryExpression
                | parenthesizedExpression
                | memberAccess
                | preIncrDecrExpression
                | postIncrDecrExpression;

            literal.Rule = number | stringLiteral | charLiteral | "true" | "false";
            parenthesizedExpression.Rule = lpar + expression + rpar;
            preIncrDecrExpression.Rule = incrOrDecr + memberAccess;
            postIncrDecrExpression.Rule = memberAccess + incrOrDecr;

            memberAccess.Rule = identifier;
            argumentListPar.Rule = lpar + argumentListOpt + rpar;
            argumentListParOpt.Rule = Empty | argumentListPar;

            binOperator.Rule = ToTerm("<") | "||" | "&&" | "|" | "^" | "&" | "==" | "!=" | ">" | "<=" | ">=" | "<<" | ">>" | "+" | "-" | "*" | "/" |
                         "%" | "=" | "+=" | "-=" | "*=" | "/=" | "%=" | "&=" | "|=" | "^=" | "<<=" | ">>=";

            // statements
            statement.Rule = declarationStatement | embeddedStatement | functionCallStatement;
            statement.ErrorRule = SyntaxError + semi;
            statementList.Rule = MakeListRule(statementList, null, statement);
            statementListOpt.Rule = Empty | statementList;

            functionCallStatement.Rule = identifier + argumentListPar;
            declarationStatement.Rule = localVariableDeclaration + semi;
            localVariableDeclaration.Rule = identifier + "=" + expression;

            embeddedStatement.Rule = block | semi | statementExpression + semi | ifStatement | iterationStatement;
            block.Rule = lbr + statementListOpt + rbr;

            ifStatement.Rule = ToTerm("if") + lpar + expression + rpar + embeddedStatement + elseClauseOpt;
            elseClauseOpt.Rule = Empty | PreferShiftHere() + "else" + embeddedStatement;

            iterationStatement.Rule = whileStatement;
            whileStatement.Rule = "while" + parenthesizedExpression + embeddedStatement;
            breakStatement.Rule = "break" + semi;

            assignmentStatement.Rule = memberAccess + assignmentOperator + expression;
            statementExpression.Rule = memberAccess | assignmentStatement | preIncrDecrExpression |
                                       postIncrDecrExpression;
            statementExpressionList.Rule = MakePlusRule(statementExpressionList, comma, statementExpression);
            incrOrDecrOpt.Rule = Empty | ToTerm("++") | "--";
            incrOrDecr.Rule = ToTerm("++") | ToTerm("--");

            Root = emitterDeclarationList;
            emitterDeclarationList.Rule = MakePlusRule(emitterDeclarationList, null, emitterDeclaration);
            emitterDeclaration.Rule = "emitter" + identifier + emitterBody;
            emitterBody.Rule = "{" + emitterMemberDeclarations + initializeDeclaration + "}";
            emitterMemberDeclaration.Rule = memberDeclaration | stateDeclaration;
            emitterMemberDeclarations.Rule = MakeStarRule(emitterMemberDeclarations, null, emitterMemberDeclaration);
            initializeDeclaration.Rule = "Initialize" + block;
            stateDeclarations.Rule = MakePlusRule(stateDeclarations, null, stateDeclaration);
            stateDeclaration.Rule = "state" + identifier + stateBody;
            stateBody.Rule = lbr + memberDeclarationsOpt + rbr;

            memberDeclaration.Rule = fieldDeclaration | functionDeclaration;
            memberDeclaration.ErrorRule = SyntaxError + ";" | SyntaxError + "}";
            memberDeclarationsOpt.Rule = MakeStarRule(memberDeclarationsOpt, null, memberDeclaration);
            fieldDeclaration.Rule = localVariableDeclaration + semi;

            functionDeclaration.Rule = "function" + identifier + formalParameterListPar + functionBody;
            formalParameterList.Rule = MakePlusRule(formalParameterList, comma, identifier);
            formalParameterListPar.Rule = lpar + rpar | lpar + formalParameterList + rpar;
            functionBody.Rule = block;

            //Prepare term set for conflict resolution
            _skipTokensInPreview.UnionWith(new Terminal[] { dot, identifier, comma, ToTerm("::"), comma, ToTerm("["), ToTerm("]") });

            //LanguageFlags = LanguageFlags.CreateAst;
        }
    }
}
