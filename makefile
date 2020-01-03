#Windows 7:
#RM=del /y

#Windows 8.1:
RM=del /S
CP=copy

#source
SRC=$(wildcard */*.cs */*/*.cs */*/*/*.cs */*/*/*/*.cs)

#config
ifeq ($(OS),Windows_NT)
FLAGS=STEAMWORKS_WIN
CSC=csc
else
FLAGS=STEAMWORKS_LIN_OSX
CSC=mcs
endif

BINDIR=bin

#output
ifeq ($(OS),Windows_NT)
OUTFILE=DecadeDiceServer.exe
else
OUTFILE=DecadeDiceServer
endif
OUTDIR=out
OUT=$(addprefix $(OUTDIR)/,$(OUTFILE))

all: $(OUTDIR)
	$(CSC) -out:$(OUT) $(SRC) -define:$(FLAGS)
	$(CP) $(BINDIR) $(OUTDIR)

$(OUTDIR):
	mkdir $(OUTDIR)

debug: clean all
release: clean all
rebuild: clean all

clean:
ifeq ($(OS),Windows_NT)
	del /S /Q *.o *.a *.exe
#	rmdir /S /Q $(OUTDIR)
else ifeq ($(shell uname), Linux)
	find . -type f -name '*.o' -exec rm -f -r -v {} \;
	find . -type f -name '*.a' -exec rm -f -r -v {} \;
#	rm $(OUTDIR)/* -f
	find . -empty -type d -delete
endif
