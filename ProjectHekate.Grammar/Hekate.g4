grammar Hekate;

/*
 * Parser Rules
 */

script
	:	(functionDeclaration|emitterUpdaterDeclaration|bulletUpdaterDeclaration)*
	;
	
variableDeclaration
	:	VAR NormalIdentifier ASSIGN expression
	;
	
functionDeclaration
	:	FUNCTION NormalIdentifier formalParameters functionBody
	;

emitterUpdaterDeclaration
	:	EUPDATER NormalIdentifier formalParameters updaterBody
	;

bulletUpdaterDeclaration
	:	BUPDATER NormalIdentifier formalParameters updaterBody
	;

functionBody
	:	block
	;

updaterBody
	:	block
	;

block
	:	LBRACE statement* RBRACE
	;
	
formalParameter
	:	NormalIdentifier
	;

formalParameterList
	:	formalParameter (COMMA formalParameter)*
	;

formalParameters
	:	LPAREN formalParameterList? RPAREN
	;

// statements
statement
	:	block	# BlockStatement
	|	IF parExpression statement (ELSE statement)?		# IfStatement
	|	FOR LPAREN forControl RPAREN statement				# ForStatement
	|	WHILE parExpression statement						# WhileStatement
	|	BREAK SEMI				# BreakStatement
	|	CONTINUE SEMI			# ContinueStatement
	|	SEMI					# EmptyStatement
	|	VAR NormalIdentifier ASSIGN createEmitterExpression SEMI	# CreateEmitterVariableStatement
	|	VAR NormalIdentifier ASSIGN buildEmitterExpression SEMI	# BuildEmitterVariableStatement
	|	variableDeclaration SEMI							# VariableDeclarationStatement
	|	expression SEMI			# ExpressionStatement
	|	NormalIdentifier ATTACH NormalIdentifier parExpressionList withUpdaterOption? SEMI		# AttachEmitterStatement
	|	FIRE NormalIdentifier parExpressionList fromEmitterOption SEMI	# FireStatement
	|	WAIT expression FRAMES SEMI		# WaitStatement
	|	RETURN expression SEMI			# ReturnStatement
	;
		
fromEmitterOption
	:	FROM NormalIdentifier
	;

forControl
	:	forInit? SEMI expression? SEMI forUpdate
	;

forInit
	:	variableDeclaration
	|	expressionList
	;

forUpdate
	:	expressionList
	;	

// expressions
parExpression
	:	LPAREN expression RPAREN
	;

expressionList
	:	expression (COMMA expression)*
	;

parExpressionList
	:	LPAREN expressionList? RPAREN
	;

updaterCallExpression
	:	NormalIdentifier parExpressionList
	;

createEmitterExpression
	:	CREATE EMITTER parExpressionList withUpdaterOption?
	;

buildEmitterExpression
	:	BUILD NormalIdentifier
	;

withUpdaterOption
	:	WITH UPDATER updaterCallExpression
	;

expression
	:	LPAREN expression RPAREN	# ParenthesizedExpression
	|	Literal						# LiteralExpression
	|	NormalIdentifier			        # NormalIdentifierExpression
	|	PropertyIdentifier					# PropertyIdentifierExpression
	|	expression Operator=(INC|DEC)		# PostIncDecExpression
	|	Operator=(
			SUB
		|	BANG
		)
		expression	# UnaryExpression
	|	expression
		Operator=(
			MUL 
		|	DIV
		|	MOD
		|	ADD
		|	SUB
		|	LE
		|	GE
		|	LT
		|	GT
		|	EQUAL
		|	NOTEQUAL
		|	AND
		|	OR
		)
		expression	# BinaryExpression
	|	(NormalIdentifier | PropertyIdentifier) 
		Operator=(
			ASSIGN
		|	MUL_ASSIGN 
		|	DIV_ASSIGN
		|	ADD_ASSIGN
		|	SUB_ASSIGN
		)
		expression  # AssignmentExpression
	|	NormalIdentifier parExpressionList		# FunctionCallExpression
	;
	
/*
 * Lexer Rules
 */

// Keywords
FUNCTION	: 'function';
CONST		: 'const';
VAR			: 'var';
FOR			: 'for';
WHILE		: 'while';
IF			: 'if';
ELSE		: 'else';
CONTINUE	: 'continue';
BREAK		: 'break';
RETURN		: 'return';

CREATE		: 'create';
EMITTER		: 'emitter';
WITH		: 'with';
UPDATER		: 'updater';
BUPDATER	: 'bulletUpdater';
EUPDATER	: 'emitterUpdater';
ATTACH		: 'attach';
BUILD		: 'build';
FIRE		: 'fire';
FROM		: 'from';
WAIT		: 'wait';
FRAMES		: 'frames';

//literals
Literal
	:	FloatingPointLiteral
	|	IntegerLiteral
	;

// Integer literals
fragment
IntegerLiteral
	:	'0'
	|	NonZeroDigit Digits*
	;

fragment
FloatingPointLiteral
	:	Digits '.' Digits
	;

fragment
Digits
	:	Digit+
	;

fragment
Digit
	:	'0'
	|	NonZeroDigit
	;

fragment
NonZeroDigit
	:	[1-9]
	;
	
// Separators
LPAREN	: '(';
RPAREN	: ')';
LBRACE	: '{';
RBRACE	: '}';
SEMI	: ';';
COMMA	: ',';

// Operators
ASSIGN		: '=';
GT			: '>';
LT			: '<';
BANG		: '!';
ADD			: '+';
SUB			: '-';
MUL			: '*';
DIV			: '/';
MOD         : '%';
EQUAL		: '==';
LE			: '<=';
GE			: '>=';
NOTEQUAL	: '!=';
AND			: '&&';
OR			: '||';
INC			: '++';
DEC			: '--';
ADD_ASSIGN	: '+=';
SUB_ASSIGN	: '-=';
MUL_ASSIGN	: '*=';
DIV_ASSIGN	: '/=';

// Identifiers
NormalIdentifier
	:	Letter LetterOrDigit*
	;

PropertyIdentifier
	:	DOLLAR NormalIdentifier
	;


fragment
Letter
	:	[a-zA-Z_]
	;

fragment
LetterOrDigit
	:	[a-zA-Z0-9_]
	;

DOLLAR	: '$';

WS 
	: [ \t\r\n\u000C]+ -> skip;

COMMENT
	: '/*' .*? '*/' -> skip;

LINE_COMMENT
	: '//' ~[\r\n]* -> skip;